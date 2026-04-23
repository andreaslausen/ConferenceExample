import { Link } from "react-router-dom";

export default function ForbiddenPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-8 text-center">
      <h1 className="text-4xl font-bold">403</h1>
      <p className="text-muted-foreground">
        Du hast keine Berechtigung für diese Seite.
      </p>
      <Link to="/" className="text-primary underline">
        Zur Startseite
      </Link>
    </div>
  );
}
