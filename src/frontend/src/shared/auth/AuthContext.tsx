import {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
  type ReactNode,
} from "react";
import keycloak from "./keycloak";

export type UserRole = "Speaker" | "Organizer" | "Attendee";

export interface AuthUser {
  id: string;
  email: string;
  role: UserRole;
}

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
  login: () => void;
  logout: () => void;
  register: () => void;
}

const APP_ROLES: UserRole[] = ["Speaker", "Organizer", "Attendee"];

function toAuthUser(): AuthUser | null {
  const parsed = keycloak.tokenParsed;
  if (!keycloak.authenticated || !parsed?.sub) return null;

  const role = parsed.realm_access?.roles.find(
    (r: string): r is UserRole => (APP_ROLES as string[]).includes(r),
  );

  return {
    id: parsed.sub,
    email: parsed.email ?? "",
    role: role ?? "Attendee",
  };
}

const AuthContext = createContext<AuthContextValue | null>(null);

// Module-scoped so React StrictMode's double-invoked effect (mount → cleanup → mount) reuses
// the same in-flight init instead of calling keycloak.init() twice, which keycloak-js rejects.
let keycloakInit: Promise<boolean> | null = null;

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    keycloak.onTokenExpired = () => {
      keycloak.updateToken(30).catch(() => keycloak.login());
    };

    keycloakInit ??= keycloak.init({
      onLoad: "check-sso",
      pkceMethod: "S256",
      silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
    });

    keycloakInit
      .then(() => {
        setUser(toAuthUser());
        setToken(keycloak.token ?? null);
      })
      .finally(() => setIsLoading(false));
  }, []);

  const login = useCallback(() => {
    keycloak.login();
  }, []);

  const logout = useCallback(() => {
    keycloak.logout({ redirectUri: window.location.origin });
  }, []);

  const register = useCallback(() => {
    keycloak.register();
  }, []);

  return (
    <AuthContext.Provider
      value={{ user, token, isLoading, login, logout, register }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
