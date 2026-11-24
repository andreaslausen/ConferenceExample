import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type MyTalk = components["schemas"]["GetMyTalksDto"];

const PAGE_SIZE = 10;

export default function MyTalksPage() {
  const { toast } = useToast();
  const [talks, setTalks] = useState<MyTalk[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    apiClient
      .GET("/api/Talks/my-talks", { params: { query: { page, pageSize: PAGE_SIZE } } })
      .then(({ data }) => {
        if (cancelled) return;
        setTalks(data?.items ?? []);
        setTotalCount(Number(data?.totalCount ?? 0));
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [page, reloadToken]);

  async function handleDelete(talk: MyTalk) {
    if (
      !window.confirm(
        `Talk „${talk.title}“ löschen? Bereits eingereichte Fassungen bleiben bei den Konferenzen erhalten.`,
      )
    ) {
      return;
    }

    const { error } = await apiClient.DELETE("/api/Talks/{id}", {
      params: { path: { id: talk.id } },
    });

    if (error) {
      toast({ title: "Talk konnte nicht gelöscht werden.", variant: "destructive" });
      return;
    }

    toast({ title: "Talk gelöscht." });
    setReloadToken((t) => t + 1);
  }

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  return (
    <PageLayout>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Meine Talks</h1>
        <Link
          to="/my-talks/new"
          className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-3 text-sm font-medium"
        >
          Neuer Talk
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
          <p className="text-muted-foreground text-base">Du hast noch keine Talks angelegt.</p>
          <Link to="/my-talks/new" className="text-primary mt-2 inline-block text-sm underline">
            Jetzt anlegen
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
                <Link
                  to={`/my-talks/${talk.id}`}
                  className="hover:text-primary truncate text-sm font-medium"
                >
                  {talk.title}
                </Link>
                <p className="text-muted-foreground mt-0.5 truncate text-xs">
                  {Number(talk.submissionCount) === 0
                    ? "Noch nicht eingereicht"
                    : `${talk.submissionCount} Einreichung${Number(talk.submissionCount) === 1 ? "" : "en"}`}
                  {talk.tags.length > 0 && ` · ${talk.tags.join(", ")}`}
                </p>
              </div>
              <div className="ml-4 flex items-center gap-2">
                <Link
                  to={`/my-talks/${talk.id}/submit`}
                  className="border-input hover:bg-accent inline-flex h-8 items-center rounded-md border px-2.5 text-xs"
                >
                  Einreichen
                </Link>
                <Link
                  to={`/my-talks/${talk.id}/edit`}
                  className="border-input hover:bg-accent inline-flex h-8 items-center rounded-md border px-2.5 text-xs"
                >
                  Bearbeiten
                </Link>
                <button
                  onClick={() => handleDelete(talk)}
                  className="border-input text-destructive hover:bg-destructive/10 inline-flex h-8 items-center rounded-md border px-2.5 text-xs"
                >
                  Löschen
                </button>
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
