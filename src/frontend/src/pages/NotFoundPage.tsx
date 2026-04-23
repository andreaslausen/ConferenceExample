import { Link } from "react-router-dom";

export default function NotFoundPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-8 text-center">
      <h1 className="text-4xl font-semibold">404</h1>
      <p className="text-muted-foreground">Diese Seite existiert nicht.</p>
      <Link to="/" className="text-primary text-sm underline">
        Zur Startseite
      </Link>
    </div>
  );
}
