import {
  createContext,
  useContext,
  useState,
  useCallback,
  type ReactNode,
} from "react";

export type ToastVariant = "default" | "destructive";

export interface Toast {
  id: number;
  title: string;
  description?: string;
  variant?: ToastVariant;
}

interface ToastContextValue {
  toasts: Toast[];
  toast: (t: Omit<Toast, "id">) => void;
  dismiss: (id: number) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

let nextId = 0;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const dismiss = useCallback((id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const toast = useCallback(
    (t: Omit<Toast, "id">) => {
      const id = ++nextId;
      setToasts((prev) => [...prev, { ...t, id }]);
      setTimeout(() => dismiss(id), 4000);
    },
    [dismiss],
  );

  return (
    <ToastContext.Provider value={{ toasts, toast, dismiss }}>
      {children}
      <ToastViewport toasts={toasts} dismiss={dismiss} />
    </ToastContext.Provider>
  );
}

function ToastViewport({
  toasts,
  dismiss,
}: {
  toasts: Toast[];
  dismiss: (id: number) => void;
}) {
  if (toasts.length === 0) return null;
  return (
    <div
      className="fixed right-4 bottom-4 z-50 flex flex-col gap-2"
      aria-live="polite"
      aria-label="Benachrichtigungen"
    >
      {toasts.map((t) => (
        <div
          key={t.id}
          role="alert"
          className={`flex w-80 items-start justify-between gap-3 rounded-lg border p-4 shadow-lg ${
            t.variant === "destructive"
              ? "border-destructive/50 bg-destructive text-destructive-foreground"
              : "bg-card text-card-foreground border-border"
          }`}
        >
          <div className="flex-1">
            <p className="text-sm font-medium">{t.title}</p>
            {t.description && (
              <p className="text-muted-foreground mt-1 text-xs">
                {t.description}
              </p>
            )}
          </div>
          <button
            onClick={() => dismiss(t.id)}
            aria-label="Schließen"
            className="hover:opacity-70 text-xs"
          >
            ✕
          </button>
        </div>
      ))}
    </div>
  );
}

export function useToast(): Pick<ToastContextValue, "toast"> {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within ToastProvider");
  return { toast: ctx.toast };
}
