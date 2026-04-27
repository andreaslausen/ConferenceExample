import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function Header() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/", { replace: true });
  }

  return (
    <header className="border-b">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3">
        <Link to="/" className="text-lg font-semibold">
          ConferenceExample
        </Link>

        <nav className="flex items-center gap-4 text-sm" aria-label="Hauptnavigation">
          {/* Public link always visible */}
          <Link
            to="/"
            className="text-muted-foreground hover:text-foreground hidden sm:inline"
          >
            Konferenzen
          </Link>

          {user?.role === "Speaker" && (
            <>
              <Link
                to="/profile"
                className="text-muted-foreground hover:text-foreground"
              >
                Profil
              </Link>
              <Link
                to="/my-talks"
                className="text-muted-foreground hover:text-foreground"
              >
                Meine Talks
              </Link>
            </>
          )}

          {user?.role === "Organizer" && (
            <Link
              to="/organizer/conferences"
              className="text-muted-foreground hover:text-foreground"
            >
              Verwaltung
            </Link>
          )}

          {user ? (
            <div className="flex items-center gap-3">
              <span className="text-muted-foreground hidden sm:inline">{user.email}</span>
              <button
                onClick={handleLogout}
                className="border-input hover:bg-accent inline-flex h-9 items-center rounded-md border px-3 text-sm"
                aria-label="Abmelden"
              >
                Abmelden
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Link
                to="/login"
                className="border-input hover:bg-accent inline-flex h-9 items-center rounded-md border px-3 text-sm"
              >
                Anmelden
              </Link>
              <Link
                to="/register"
                className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-3 text-sm"
              >
                Registrieren
              </Link>
            </div>
          )}
        </nav>
      </div>
    </header>
  );
}
