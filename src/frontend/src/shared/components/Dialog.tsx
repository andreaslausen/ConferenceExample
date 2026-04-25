import { useState, type ReactNode } from "react";

interface Props {
  open: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  onConfirm: () => void;
  confirmLabel?: string;
  variant?: "default" | "destructive";
}

export function ConfirmDialog({
  open,
  onClose,
  title,
  description,
  onConfirm,
  confirmLabel = "Bestätigen",
  variant = "default",
}: Props) {
  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center"
      role="dialog"
      aria-modal="true"
      aria-labelledby="confirm-title"
    >
      <div
        className="fixed inset-0 bg-black/50"
        onClick={onClose}
        aria-hidden="true"
      />
      <div className="bg-card border-border relative z-10 w-full max-w-sm rounded-lg border p-6 shadow-lg">
        <h2 id="confirm-title" className="mb-2 text-base font-semibold">
          {title}
        </h2>
        {description && (
          <p className="text-muted-foreground mb-4 text-sm">{description}</p>
        )}
        <div className="flex justify-end gap-2">
          <button
            onClick={onClose}
            className="border-input hover:bg-accent inline-flex h-9 items-center rounded-md border px-3 text-sm"
          >
            Abbrechen
          </button>
          <button
            onClick={() => {
              onConfirm();
              onClose();
            }}
            className={`inline-flex h-9 items-center rounded-md px-3 text-sm font-medium ${
              variant === "destructive"
                ? "bg-destructive text-destructive-foreground hover:bg-destructive/90"
                : "bg-primary text-primary-foreground hover:bg-primary/90"
            }`}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}

export function Sheet({
  open,
  onClose,
  title,
  children,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
}) {
  if (!open) return null;
  return (
    <div
      className="fixed inset-0 z-50 flex justify-end"
      role="dialog"
      aria-modal="true"
      aria-labelledby="sheet-title"
    >
      <div
        className="fixed inset-0 bg-black/50"
        onClick={onClose}
        aria-hidden="true"
      />
      <div className="bg-card border-border relative z-10 flex h-full w-full max-w-md flex-col border-l shadow-xl">
        <div className="flex items-center justify-between border-b p-4">
          <h2 id="sheet-title" className="text-base font-semibold">
            {title}
          </h2>
          <button
            onClick={onClose}
            aria-label="Schließen"
            className="hover:bg-accent rounded-md p-1 text-sm"
          >
            ✕
          </button>
        </div>
        <div className="flex-1 overflow-y-auto p-4">{children}</div>
      </div>
    </div>
  );
}

export function useDisclosure(initial = false) {
  const [isOpen, setIsOpen] = useState(initial);
  return {
    isOpen,
    open: () => setIsOpen(true),
    close: () => setIsOpen(false),
    toggle: () => setIsOpen((v) => !v),
  };
}
