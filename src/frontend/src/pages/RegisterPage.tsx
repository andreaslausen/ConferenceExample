import { useState, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import * as RadioGroupPrimitive from "@radix-ui/react-radio-group";
import { useAuth } from "../shared/auth/AuthContext";

const ROLES = [
  { label: "Speaker", value: 0 },
  { label: "Organizer", value: 1 },
] as const;

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<number>(0);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await register(email, password, role);
      navigate("/", { replace: true });
    } catch {
      setError("Registrierung fehlgeschlagen. Bitte versuche es erneut.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-sm space-y-6">
        <div className="space-y-1 text-center">
          <h1 className="text-2xl font-semibold">Registrieren</h1>
          <p className="text-muted-foreground text-sm">
            Erstelle dein Konto für die Konferenzplattform.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <label htmlFor="email" className="text-sm font-medium">
              E-Mail
            </label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>

          <div className="space-y-1.5">
            <label htmlFor="password" className="text-sm font-medium">
              Passwort
            </label>
            <input
              id="password"
              type="password"
              autoComplete="new-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>

          <fieldset className="space-y-2">
            <legend className="text-sm font-medium">Rolle</legend>
            <RadioGroupPrimitive.Root
              value={String(role)}
              onValueChange={(v) => setRole(Number(v))}
              className="flex gap-4"
              aria-label="Rolle auswählen"
            >
              {ROLES.map(({ label, value }) => (
                <div key={value} className="flex items-center gap-2">
                  <RadioGroupPrimitive.Item
                    value={String(value)}
                    id={`role-${value}`}
                    className="border-primary text-primary focus-visible:ring-ring aspect-square h-4 w-4 rounded-full border focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    <RadioGroupPrimitive.Indicator className="flex items-center justify-center">
                      <div className="bg-primary h-2 w-2 rounded-full" />
                    </RadioGroupPrimitive.Indicator>
                  </RadioGroupPrimitive.Item>
                  <label htmlFor={`role-${value}`} className="text-sm">
                    {label}
                  </label>
                </div>
              ))}
            </RadioGroupPrimitive.Root>
          </fieldset>

          {error && (
            <p role="alert" className="text-destructive text-sm">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loading}
            className="bg-primary text-primary-foreground hover:bg-primary/90 focus-visible:ring-ring inline-flex h-10 w-full items-center justify-center rounded-md px-4 py-2 text-sm font-medium focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:pointer-events-none disabled:opacity-50"
          >
            {loading ? "Laden…" : "Konto erstellen"}
          </button>
        </form>

        <p className="text-muted-foreground text-center text-sm">
          Bereits registriert?{" "}
          <Link to="/login" className="text-primary underline">
            Anmelden
          </Link>
        </p>
      </div>
    </div>
  );
}
