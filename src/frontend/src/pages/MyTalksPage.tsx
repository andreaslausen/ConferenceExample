import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout } from "../shared/components/Layout";

type MyTalk = components["schemas"]["GetMyTalksDto"];

const PAGE_SIZE = 10;

const STATUS_LABELS: Record<string, string> = {
  Pending: "Ausstehend",
  Accepted: "Angenommen",
  Rejected: "Abgelehnt",
};

const STATUS_COLORS: Record<string, string> = {
  Pending: "bg-muted text-muted-foreground",
  Accepted: "bg-primary/10 text-primary",
  Rejected: "bg-destructive/10 text-destructive",
};

function StatusBadge({ status }: { status: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${STATUS_COLORS[status] ?? "bg-muted text-muted-foreground"}`}
    >
      {STATUS_LABELS[status] ?? status}
    </span>
  );
}

export default function MyTalksPage() {
  const [talks, setTalks] = useState<MyTalk[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    apiClient
      .GET("/api/Talks/my-talks", {
        params: { query: { page, pageSize: PAGE_SIZE } },
      })
      .then(({ data }) => {
        if (cancelled) return;
        setTalks(data?.items ?? []);
        setTotalCount(Number(data?.totalCount ?? 0));
        setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [page]);

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  return (
    <PageLayout>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Meine Talks</h1>
        <Link
          to="/my-talks/submit"
          className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-3 text-sm font-medium"
        >
          Talk einreichen
        </Link>
      </div>

      {loading ? (
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-16 w-full rounded-lg" />
          ))}
        </div>
      ) : talks.length === 0 ? (
        <div className="py-20 text-center">
          <p className="text-muted-foreground text-base">
            Du hast noch keine Talks eingereicht.
          </p>
          <Link
            to="/my-talks/submit"
            className="text-primary mt-2 inline-block text-sm underline"
          >
            Jetzt einreichen
          </Link>
        </div>
      ) : (
        <div className="space-y-2">
          {talks.map((talk) => (
            <div
              key={talk.id}
              className="border-border flex items-center justify-between rounded-lg border p-4"
            >
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{talk.title}</p>
                {talk.tags.length > 0 && (
                  <p className="text-muted-foreground mt-0.5 truncate text-xs">
                    {talk.tags.join(", ")}
                  </p>
                )}
              </div>
              <div className="ml-4 flex items-center gap-3">
                <StatusBadge status={talk.status} />
                <Link
                  to={`/my-talks/${talk.id}/edit`}
                  className="border-input hover:bg-accent inline-flex h-8 items-center rounded-md border px-2.5 text-xs"
                >
                  Bearbeiten
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}

      {!loading && totalPages > 1 && (
        <div className="mt-8 flex items-center justify-center gap-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="border-input hover:bg-accent inline-flex h-9 items-center rounded-md border px-3 text-sm disabled:pointer-events-none disabled:opacity-50"
          >
            Zurück
          </button>
          <span className="text-muted-foreground text-sm">
            Seite {page} von {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="border-input hover:bg-accent inline-flex h-9 items-center rounded-md border px-3 text-sm disabled:pointer-events-none disabled:opacity-50"
          >
            Weiter
          </button>
        </div>
      )}
    </PageLayout>
  );
}
