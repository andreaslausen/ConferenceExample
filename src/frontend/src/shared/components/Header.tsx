import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useTheme } from "../auth/ThemeContext";
import type { Theme } from "../auth/ThemeContext";

const THEMES: { id: Theme; label: string; swatch: string }[] = [
  {
    id: "aurora",
    label: "Aurora",
    swatch: "linear-gradient(135deg, #7c3aed, #06b6d4)",
  },
  {
    id: "sunrise",
    label: "Sunrise",
    swatch: "linear-gradient(135deg, #f97316, #ec4899, #8b5cf6)",
  },
  {
    id: "ocean",
    label: "Ocean",
    swatch: "linear-gradient(135deg, #1e3a8a, #0ea5e9)",
  },
];

function ThemeSwitcher() {
  const { theme, setTheme } = useTheme();
  return (
    <div
      className="flex items-center gap-1.5"
      role="group"
      aria-label="Design wählen"
    >
      {THEMES.map((t) => (
        <button
          key={t.id}
          onClick={() => setTheme(t.id)}
          title={t.label}
          aria-label={`Design: ${t.label}`}
          aria-pressed={theme === t.id}
          className={`h-[18px] w-[18px] rounded-full transition-all duration-200 ${
            theme === t.id
              ? "scale-125 shadow-md ring-2 ring-white/80"
              : "opacity-50 hover:opacity-80 hover:scale-110"
          }`}
          style={{ background: t.swatch }}
        />
      ))}
    </div>
  );
}

export function Header() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/", { replace: true });
  }

  return (
    <header
      style={{ background: "var(--header-gradient)" }}
      className="shadow-md"
    >
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3.5">
        <Link
          to="/"
          className="text-lg font-bold tracking-tight text-white drop-shadow-sm"
        >
          ConferenceExample
        </Link>

        <nav
          className="flex items-center gap-4 text-sm"
          aria-label="Hauptnavigation"
        >
          <Link
            to="/"
            className="hidden text-white/80 transition-colors hover:text-white sm:inline"
          >
            Konferenzen
          </Link>

          {user?.role === "Speaker" && (
            <>
              <Link
                to="/profile"
                className="text-white/80 transition-colors hover:text-white"
              >
                Profil
              </Link>
              <Link
                to="/my-talks"
                className="text-white/80 transition-colors hover:text-white"
              >
                Meine Talks
              </Link>
            </>
          )}

          {user?.role === "Organizer" && (
            <Link
              to="/organizer/conferences"
              className="text-white/80 transition-colors hover:text-white"
            >
              Verwaltung
            </Link>
          )}

          {user ? (
            <div className="flex items-center gap-3">
              <span className="hidden text-xs text-white/60 sm:inline">
                {user.email}
              </span>
              <button
                onClick={handleLogout}
                className="inline-flex h-8 items-center rounded-md bg-white/15 px-3 text-sm text-white transition-colors hover:bg-white/25"
                aria-label="Abmelden"
              >
                Abmelden
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Link
                to="/login"
                className="inline-flex h-8 items-center rounded-md bg-white/15 px-3 text-sm text-white transition-colors hover:bg-white/25"
              >
                Anmelden
              </Link>
              <Link
                to="/register"
                className="inline-flex h-8 items-center rounded-md bg-white px-3 text-sm font-semibold transition-colors hover:bg-white/90"
                style={{ color: "var(--primary)" }}
              >
                Registrieren
              </Link>
            </div>
          )}

          <ThemeSwitcher />
        </nav>
      </div>
    </header>
  );
}
