import {
  createContext,
  useContext,
  useState,
  useCallback,
  type ReactNode,
} from "react";
import apiClient, { getToken, setToken, clearToken } from "../api/client";

export type UserRole = "Speaker" | "Organizer" | "Attendee";

export interface AuthUser {
  id: string;
  email: string;
  role: UserRole;
}

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  register: (
    email: string,
    password: string,
    role: number,
  ) => Promise<void>;
}

function decodeJwt(token: string): AuthUser {
  const payload = JSON.parse(atob(token.split(".")[1])) as Record<
    string,
    string
  >;
  return {
    id: payload["sub"],
    email: payload["email"],
    role: payload[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ] as UserRole ?? payload["role"] as UserRole,
  };
}

function restoreUser(): { token: string; user: AuthUser } | null {
  const token = getToken();
  if (!token) return null;
  try {
    return { token, user: decodeJwt(token) };
  } catch {
    clearToken();
    return null;
  }
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const restored = restoreUser();
  const [token, setTokenState] = useState<string | null>(
    restored?.token ?? null,
  );
  const [user, setUser] = useState<AuthUser | null>(restored?.user ?? null);

  const login = useCallback(async (email: string, password: string) => {
    const { data, error } = await apiClient.POST("/api/Auth/login", {
      body: { email, password },
    });
    if (error || !data) {
      throw new Error("Ungültige Anmeldedaten");
    }
    const jwt = data.token;
    setToken(jwt);
    setTokenState(jwt);
    setUser(decodeJwt(jwt));
  }, []);

  const logout = useCallback(() => {
    clearToken();
    setTokenState(null);
    setUser(null);
  }, []);

  const register = useCallback(
    async (email: string, password: string, role: number) => {
      const { data, error } = await apiClient.POST("/api/Auth/register", {
        body: { email, password, role },
      });
      if (error || !data) {
        throw new Error("Registrierung fehlgeschlagen");
      }
      const jwt = data.token;
      setToken(jwt);
      setTokenState(jwt);
      setUser(decodeJwt(jwt));
    },
    [],
  );

  return (
    <AuthContext.Provider value={{ user, token, login, logout, register }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
