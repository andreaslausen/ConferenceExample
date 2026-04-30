import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout } from "../shared/components/Layout";

type Conference = components["schemas"]["GetMyConferencesDto"];

const PAGE_SIZE = 15;

const STATUS_LABELS: Record<string, string> = {
  Draft: "Entwurf",
  CallForSpeakers: "Call for Speakers",
  CallForSpeakersClosed: "Call for Speakers geschlossen",
  ProgramPublished: "Programm veröffentlicht",
};

function formatDate(iso: string): string {
  return new Intl.DateTimeFormat("de-DE", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(iso));
}

export default function OrganizerConferenceListPage() {
  const [conferences, setConferences] = useState<Conference[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    apiClient
      .GET("/api/Conferences/my", { params: { query: { page, pageSize: PAGE_SIZE } } })
      .then(({ data }) => {
        if (cancelled) return;
        setConferences(data?.items ?? []);
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
        <h1 className="text-2xl font-semibold">Konferenzen</h1>
        <Link
          to="/organizer/conferences/new"
          className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-3 text-sm font-medium"
        >
          Neue Konferenz
        </Link>
      </div>

      {loading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full rounded-md" />
          ))}
        </div>
      ) : conferences.length === 0 ? (
        <div className="py-20 text-center">
          <p className="text-muted-foreground">Noch keine Konferenzen angelegt.</p>
          <Link
            to="/organizer/conferences/new"
            className="text-primary mt-2 inline-block text-sm underline"
          >
            Erste Konferenz erstellen
          </Link>
        </div>
      ) : (
        <div className="border-border overflow-hidden rounded-lg border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Name</th>
                <th className="px-4 py-3 text-left font-medium">Datum</th>
                <th className="px-4 py-3 text-left font-medium">Ort</th>
                <th className="px-4 py-3 text-left font-medium">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y">
              {conferences.map((c) => (
                <tr key={c.id} className="hover:bg-muted/30">
                  <td className="px-4 py-3 font-medium">{c.name}</td>
                  <td className="text-muted-foreground px-4 py-3">
                    {formatDate(c.startDate)} – {formatDate(c.endDate)}
                  </td>
                  <td className="text-muted-foreground px-4 py-3">
                    {c.city}, {c.country}
                  </td>
                  <td className="px-4 py-3">
                    <span className="text-muted-foreground text-xs">
                      {STATUS_LABELS[c.status] ?? c.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Link
                      to={`/organizer/conferences/${c.id}`}
                      className="text-primary text-xs underline-offset-2 hover:underline"
                    >
                      Verwalten
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {!loading && totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-2">
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
