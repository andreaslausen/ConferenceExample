import { Component, type ReactNode } from "react";

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false, error: null };

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  render() {
    if (this.state.hasError) {
      return (
        this.props.fallback ?? (
          <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-8 text-center">
            <h1 className="text-2xl font-semibold">Etwas ist schiefgelaufen</h1>
            <p className="text-muted-foreground text-sm">
              {this.state.error?.message ?? "Ein unbekannter Fehler ist aufgetreten."}
            </p>
            <button
              className="text-primary text-sm underline"
              onClick={() => this.setState({ hasError: false, error: null })}
            >
              Erneut versuchen
            </button>
          </div>
        )
      );
    }
    return this.props.children;
  }
}
