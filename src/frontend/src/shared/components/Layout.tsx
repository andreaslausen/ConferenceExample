import type { ReactNode } from "react";
import { useNavigate } from "react-router-dom";

interface BreadcrumbItem {
  label: string;
  to?: string;
}

interface Props {
  items: BreadcrumbItem[];
}

export function Breadcrumbs({ items }: Props) {
  const navigate = useNavigate();
  return (
    <nav aria-label="Breadcrumb" className="mb-4 flex flex-wrap items-center gap-1 text-sm text-muted-foreground">
      {items.map((item, i) => {
        const isLast = i === items.length - 1;
        return (
          <span key={i} className="flex items-center gap-1">
            {i > 0 && <span aria-hidden>/</span>}
            {isLast || !item.to ? (
              <span className={isLast ? "text-foreground font-medium" : ""}>{item.label}</span>
            ) : (
              <button
                onClick={() => navigate(item.to!)}
                className="hover:text-foreground underline-offset-2 hover:underline"
              >
                {item.label}
              </button>
            )}
          </span>
        );
      })}
    </nav>
  );
}

export function PageLayout({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <div className={`mx-auto max-w-7xl px-4 py-8 ${className ?? ""}`}>
      {children}
    </div>
  );
}
