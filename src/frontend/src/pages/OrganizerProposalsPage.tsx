import { useState, useEffect, useCallback } from "react";
import { useParams } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { Sheet } from "../shared/components/Dialog";
import { useToast } from "../shared/components/Toast";

type Talk = components["schemas"]["GetConferenceTalksDto"];

const PAGE_SIZE = 15;

type StatusFilter = "All" | "Pending" | "Accepted" | "Rejected";

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

export default function OrganizerProposalsPage() {
  const { id } = useParams<{ id: string }>();
  const { toast } = useToast();

  const [talks, setTalks] = useState<Talk[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState<StatusFilter>("All");
  const [loading, setLoading] = useState(true);
  const [selectedTalk, setSelectedTalk] = useState<Talk | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const load = useCallback(() => {
    if (!id) return;
    setLoading(true);
    apiClient
      .GET("/api/Conferences/{id}/talks", {
        params: { query: { page, pageSize: PAGE_SIZE }, path: { id } },
      })
      .then(({ data }) => {
        setTalks(data?.items ?? []);
        setTotalCount(Number(data?.totalCount ?? 0));
        setLoading(false);
      });
  }, [id, page]);

  useEffect(() => {
    load();
  }, [load]);

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  const filtered =
    filter === "All" ? talks : talks.filter((t) => t.status === filter);

  async function accept(talk: Talk) {
    if (!id) return;
    setActionLoading(talk.id);
    const { error } = await apiClient.PUT(
      "/api/Conferences/{conferenceId}/talks/{talkId}/accept",
      { params: { path: { conferenceId: id, talkId: talk.id } } },
    );
    setActionLoading(null);
    if (error) {
      toast({ title: "Annehmen fehlgeschlagen.", variant: "destructive" });
      return;
    }
    setTalks((prev) =>
      prev.map((t) => (t.id === talk.id ? { ...t, status: "Accepted" } : t)),
    );
    if (selectedTalk?.id === talk.id) {
      setSelectedTalk({ ...talk, status: "Accepted" });
    }
    toast({ title: `„${talk.title}" angenommen.` });
  }

  async function reject(talk: Talk) {
    if (!id) return;
    setActionLoading(talk.id);
    const { error } = await apiClient.PUT(
      "/api/Conferences/{conferenceId}/talks/{talkId}/reject",
      { params: { path: { conferenceId: id, talkId: talk.id } } },
    );
    setActionLoading(null);
    if (error) {
      toast({ title: "Ablehnen fehlgeschlagen.", variant: "destructive" });
      return;
    }
    setTalks((prev) =>
      prev.map((t) => (t.id === talk.id ? { ...t, status: "Rejected" } : t)),
    );
    if (selectedTalk?.id === talk.id) {
      setSelectedTalk({ ...talk, status: "Rejected" });
    }
    toast({ title: `„${talk.title}" abgelehnt.` });
  }

  const FILTER_TABS: { value: StatusFilter; label: string }[] = [
    { value: "All", label: "Alle" },
    { value: "Pending", label: "Ausstehend" },
    { value: "Accepted", label: "Angenommen" },
    { value: "Rejected", label: "Abgelehnt" },
  ];

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Konferenzen", to: "/organizer/conferences" },
          { label: "Konferenz", to: `/organizer/conferences/${id}` },
          { label: "Einreichungen" },
        ]}
      />
      <h1 className="mb-4 text-2xl font-semibold">Einreichungen</h1>

      {/* Filter tabs */}
      <div
        className="mb-4 flex gap-1 border-b"
        role="tablist"
        aria-label="Statusfilter"
      >
        {FILTER_TABS.map((tab) => (
          <button
            key={tab.value}
            role="tab"
            aria-selected={filter === tab.value}
            onClick={() => {
              setFilter(tab.value);
              setPage(1);
            }}
            className={`px-3 py-2 text-sm font-medium transition-colors ${
              filter === tab.value
                ? "border-b-2 border-primary text-foreground"
                : "text-muted-foreground hover:text-foreground"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-14 w-full" />
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <div className="py-16 text-center">
          <p className="text-muted-foreground text-sm">
            Keine Einreichungen in dieser Kategorie.
          </p>
        </div>
      ) : (
        <div className="border-border overflow-hidden rounded-lg border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Titel</th>
                <th className="px-4 py-3 text-left font-medium">Speaker</th>
                <th className="px-4 py-3 text-left font-medium">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y">
              {filtered.map((talk) => (
                <tr
                  key={talk.id}
                  className="hover:bg-muted/30 cursor-pointer"
                  onClick={() => setSelectedTalk(talk)}
                >
                  <td className="max-w-xs truncate px-4 py-3 font-medium">
                    {talk.title}
                  </td>
                  <td className="text-muted-foreground px-4 py-3">
                    {talk.speakerName}
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={talk.status} />
                  </td>
                  <td
                    className="px-4 py-3 text-right"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <div className="flex justify-end gap-1">
                      {talk.status !== "Accepted" && (
                        <button
                          onClick={() => accept(talk)}
                          disabled={actionLoading === talk.id}
                          aria-label={`${talk.title} annehmen`}
                          className="inline-flex h-7 items-center rounded-md bg-primary/10 px-2 text-xs font-medium text-primary hover:bg-primary/20 disabled:opacity-50"
                        >
                          Annehmen
                        </button>
                      )}
                      {talk.status !== "Rejected" && (
                        <button
                          onClick={() => reject(talk)}
                          disabled={actionLoading === talk.id}
                          aria-label={`${talk.title} ablehnen`}
                          className="text-destructive inline-flex h-7 items-center rounded-md bg-destructive/10 px-2 text-xs font-medium hover:bg-destructive/20 disabled:opacity-50"
                        >
                          Ablehnen
                        </button>
                      )}
                    </div>
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

      {/* Detail Sheet */}
      <Sheet
        open={selectedTalk !== null}
        onClose={() => setSelectedTalk(null)}
        title={selectedTalk?.title ?? ""}
      >
        {selectedTalk && (
          <div className="space-y-4">
            <div>
              <p className="text-muted-foreground text-xs font-medium uppercase tracking-wide">
                Speaker
              </p>
              <p className="mt-1 text-sm">{selectedTalk.speakerName}</p>
            </div>

            <div>
              <p className="text-muted-foreground text-xs font-medium uppercase tracking-wide">
                Status
              </p>
              <div className="mt-1">
                <StatusBadge status={selectedTalk.status} />
              </div>
            </div>

            {selectedTalk.tags.length > 0 && (
              <div>
                <p className="text-muted-foreground text-xs font-medium uppercase tracking-wide">
                  Tags
                </p>
                <div className="mt-1 flex flex-wrap gap-1">
                  {selectedTalk.tags.map((tag) => (
                    <span
                      key={tag}
                      className="bg-secondary text-secondary-foreground rounded-full px-2.5 py-0.5 text-xs"
                    >
                      {tag}
                    </span>
                  ))}
                </div>
              </div>
            )}

            <div>
              <p className="text-muted-foreground text-xs font-medium uppercase tracking-wide">
                Abstract
              </p>
              <p className="mt-1 text-sm leading-relaxed whitespace-pre-wrap">
                {selectedTalk.abstract}
              </p>
            </div>

            <div className="flex gap-2 pt-2">
              {selectedTalk.status !== "Accepted" && (
                <button
                  onClick={() => accept(selectedTalk)}
                  disabled={actionLoading === selectedTalk.id}
                  className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-3 text-sm font-medium disabled:opacity-50"
                >
                  Annehmen
                </button>
              )}
              {selectedTalk.status !== "Rejected" && (
                <button
                  onClick={() => reject(selectedTalk)}
                  disabled={actionLoading === selectedTalk.id}
                  className="bg-destructive text-destructive-foreground hover:bg-destructive/90 inline-flex h-9 items-center rounded-md px-3 text-sm font-medium disabled:opacity-50"
                >
                  Ablehnen
                </button>
              )}
            </div>
          </div>
        )}
      </Sheet>
    </PageLayout>
  );
}
