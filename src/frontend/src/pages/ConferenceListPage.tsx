import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { Calendar, MapPin } from "lucide-react";
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
    CallForSpeakersClosed:
      "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400",
    ProgramPublished:
      "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400",
  };
  return (
    <span
      className={`inline-flex shrink-0 items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${colors[status] ?? "bg-muted text-muted-foreground"}`}
    >
      {labels[status] ?? status}
    </span>
  );
}

function ConferenceCard({ conference }: { conference: Conference }) {
  return (
    <Link
      to={`/conferences/${conference.id}`}
      className="conference-card block rounded-xl p-6"
    >
      <div className="mb-4 flex items-start justify-between gap-3">
        <h2 className="text-base font-semibold leading-snug">
          {conference.name}
        </h2>
        <StatusBadge status={conference.status} />
      </div>
      <div className="space-y-1.5">
        <p className="flex items-center gap-2 text-sm text-muted-foreground">
          <Calendar size={13} className="shrink-0" />
          {formatDateRange(conference.startDate, conference.endDate)}
        </p>
        <p className="flex items-center gap-2 text-sm text-muted-foreground">
          <MapPin size={13} className="shrink-0" />
          {conference.city}, {conference.country}
        </p>
      </div>
    </Link>
  );
}

function ConferenceCardSkeleton() {
  return (
    <div className="rounded-xl border p-6" style={{ borderColor: "var(--border)" }}>
      <div className="mb-4 flex items-start justify-between gap-3">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-5 w-16 rounded-full" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-1/2" />
        <Skeleton className="h-4 w-2/5" />
      </div>
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
    <div className="mx-auto max-w-7xl px-4 py-10">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">Konferenzen</h1>
        <p className="mt-1.5 text-sm text-muted-foreground">
          Entdecke kommende Veranstaltungen und melde dich an.
        </p>
      </div>

      {error && (
        <p role="alert" className="mb-4 text-sm text-destructive">
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
          <p className="text-base text-muted-foreground">
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
        <div className="mt-10 flex items-center justify-center gap-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="inline-flex h-9 items-center rounded-md border border-input bg-background px-3 text-sm transition-colors hover:bg-accent disabled:pointer-events-none disabled:opacity-50"
          >
            Zurück
          </button>
          <span className="text-sm text-muted-foreground">
            Seite {page} von {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="inline-flex h-9 items-center rounded-md border border-input bg-background px-3 text-sm transition-colors hover:bg-accent disabled:pointer-events-none disabled:opacity-50"
          >
            Weiter
          </button>
        </div>
      )}
    </div>
  );
}
