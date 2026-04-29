import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";

type Conference = components["schemas"]["GetAllConferencesDto"];

const PAGE_SIZE = 9;

function formatDateRange(start: string, end: string): string {
  const fmt = new Intl.DateTimeFormat("de-DE", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
  return `${fmt.format(new Date(start))} – ${fmt.format(new Date(end))}`;
}

function StatusBadge({ status }: { status: string }) {
  const labels: Record<string, string> = {
    Draft: "Entwurf",
    CallForSpeakers: "Call for Speakers",
    CallForSpeakersClosed: "Call for Speakers geschlossen",
    ProgramPublished: "Programm veröffentlicht",
  };
  const colors: Record<string, string> = {
    Draft: "bg-muted text-muted-foreground",
    CallForSpeakers: "bg-primary/10 text-primary",
    CallForSpeakersClosed: "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400",
    ProgramPublished: "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400",
  };
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${colors[status] ?? "bg-muted text-muted-foreground"}`}
    >
      {labels[status] ?? status}
    </span>
  );
}

function ConferenceCard({ conference }: { conference: Conference }) {
  return (
    <Link
      to={`/conferences/${conference.id}`}
      className="border-border hover:border-primary/50 hover:shadow-sm block rounded-lg border bg-card p-5 transition-all"
    >
      <div className="mb-3 flex items-start justify-between gap-2">
        <h2 className="text-base font-semibold leading-tight text-card-foreground">
          {conference.name}
        </h2>
        <StatusBadge status={conference.status} />
      </div>
      <p className="text-muted-foreground mb-1 text-sm">
        {formatDateRange(conference.startDate, conference.endDate)}
      </p>
      <p className="text-muted-foreground text-sm">
        {conference.city}, {conference.country}
      </p>
    </Link>
  );
}

function ConferenceCardSkeleton() {
  return (
    <div className="rounded-lg border p-5">
      <div className="mb-3 flex items-start justify-between">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-5 w-16 rounded-full" />
      </div>
      <Skeleton className="mb-1 h-4 w-1/2" />
      <Skeleton className="h-4 w-2/5" />
    </div>
  );
}

export default function ConferenceListPage() {
  const [conferences, setConferences] = useState<Conference[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    apiClient
      .GET("/api/Conferences", {
        params: { query: { page, pageSize: PAGE_SIZE } },
      })
      .then(({ data, error: apiError }) => {
        if (cancelled) return;
        if (apiError || !data) {
          setError("Konferenzen konnten nicht geladen werden.");
          return;
        }
        setConferences(data.items);
        setTotalCount(Number(data.totalCount));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [page]);

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-semibold">Konferenzen</h1>

      {error && (
        <p role="alert" className="text-destructive mb-4 text-sm">
          {error}
        </p>
      )}

      {loading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: PAGE_SIZE }).map((_, i) => (
            <ConferenceCardSkeleton key={i} />
          ))}
        </div>
      ) : conferences.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <p className="text-muted-foreground text-base">
            Keine Konferenzen vorhanden.
          </p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {conferences.map((c) => (
            <ConferenceCard key={c.id} conference={c} />
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
    </div>
  );
}
