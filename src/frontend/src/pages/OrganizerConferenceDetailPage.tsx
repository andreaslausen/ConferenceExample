import { useState, useEffect, type FormEvent, type KeyboardEvent } from "react";
import { useParams, Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type ConferenceDetail = components["schemas"]["GetConferenceByIdDto"];

const STATUSES = [
  { value: 0, label: "Entwurf" },
  { value: 1, label: "Veröffentlicht" },
  { value: 2, label: "Abgeschlossen" },
];

const STATUS_NAMES: Record<string, string> = {
  Draft: "Entwurf",
  Published: "Veröffentlicht",
  Completed: "Abgeschlossen",
};

function formatDate(iso: string): string {
  return new Intl.DateTimeFormat("de-DE", {
    day: "2-digit",
    month: "long",
    year: "numeric",
  }).format(new Date(iso));
}

export default function OrganizerConferenceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { toast } = useToast();

  const [conference, setConference] = useState<ConferenceDetail | null>(null);
  const [loading, setLoading] = useState(true);

  const [renaming, setRenaming] = useState(false);
  const [nameInput, setNameInput] = useState("");
  const [savingName, setSavingName] = useState(false);

  const [changingStatus, setChangingStatus] = useState(false);

  useEffect(() => {
    if (!id) return;
    apiClient
      .GET("/api/Conferences/{id}", { params: { path: { id } } })
      .then(({ data }) => {
        setConference(data ?? null);
        setLoading(false);
      });
  }, [id]);

  async function saveName(e?: FormEvent) {
    e?.preventDefault();
    if (!id || !nameInput.trim()) return;
    setSavingName(true);
    const { error } = await apiClient.PUT("/api/Conferences/{id}/name", {
      params: { path: { id } },
      body: { name: nameInput.trim() },
    });
    setSavingName(false);
    if (error) {
      toast({ title: "Umbenennung fehlgeschlagen.", variant: "destructive" });
      return;
    }
    setConference((c) => c ? { ...c, name: nameInput.trim() } : c);
    setRenaming(false);
    toast({ title: "Name gespeichert." });
  }

  function startRenaming() {
    setNameInput(conference?.name ?? "");
    setRenaming(true);
  }

  function handleNameKeyDown(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Escape") {
      setRenaming(false);
    }
  }

  async function handleStatusChange(statusValue: number) {
    if (!id) return;
    setChangingStatus(true);
    const { error } = await apiClient.PUT("/api/Conferences/{id}/status", {
      params: { path: { id } },
      body: { status: statusValue },
    });
    setChangingStatus(false);
    if (error) {
      toast({ title: "Statusänderung fehlgeschlagen.", variant: "destructive" });
      return;
    }
    const newStatus = STATUSES.find((s) => s.value === statusValue);
    const newStatusName = newStatus?.label ?? "";
    setConference((c) => c ? { ...c, status: newStatusName } : c);
    toast({ title: `Status auf "${newStatusName}" gesetzt.` });
  }

  if (loading) {
    return (
      <PageLayout>
        <Skeleton className="mb-2 h-8 w-64" />
        <Skeleton className="mb-6 h-4 w-48" />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-20 rounded-lg" />
          ))}
        </div>
      </PageLayout>
    );
  }

  if (!conference) {
    return (
      <PageLayout>
        <p className="text-destructive text-sm">Konferenz nicht gefunden.</p>
      </PageLayout>
    );
  }

  const currentStatusIndex = STATUSES.findIndex(
    (s) => s.label === STATUS_NAMES[conference.status],
  );

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Konferenzen", to: "/organizer/conferences" },
          { label: conference.name },
        ]}
      />

      {/* Name with inline rename */}
      <div className="mb-1 flex items-center gap-2">
        {renaming ? (
          <form onSubmit={saveName} className="flex items-center gap-2">
            <input
              autoFocus
              value={nameInput}
              onChange={(e) => setNameInput(e.target.value)}
              onKeyDown={handleNameKeyDown}
              className="border-input bg-background focus-visible:ring-ring h-9 rounded-md border px-3 text-xl font-semibold focus-visible:ring-2 focus-visible:outline-none"
            />
            <button
              type="submit"
              disabled={savingName}
              className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-3 text-sm disabled:opacity-50"
            >
              {savingName ? "…" : "OK"}
            </button>
            <button
              type="button"
              onClick={() => setRenaming(false)}
              className="border-input hover:bg-accent inline-flex h-9 items-center rounded-md border px-3 text-sm"
            >
              ✕
            </button>
          </form>
        ) : (
          <>
            <h1 className="text-2xl font-semibold">{conference.name}</h1>
            <button
              onClick={startRenaming}
              aria-label="Name bearbeiten"
              className="text-muted-foreground hover:text-foreground text-sm"
            >
              ✎
            </button>
          </>
        )}
      </div>

      <p className="text-muted-foreground mb-4 text-sm">
        {formatDate(conference.startDate)} – {formatDate(conference.endDate)} ·{" "}
        {conference.locationName}, {conference.city}
      </p>

      {/* Status change */}
      <div className="mb-8 flex items-center gap-2">
        <span className="text-sm font-medium">Status:</span>
        <div className="flex rounded-md border" role="group" aria-label="Status ändern">
          {STATUSES.map((s, i) => {
            const isActive = i === currentStatusIndex;
            return (
              <button
                key={s.value}
                onClick={() => handleStatusChange(s.value)}
                disabled={changingStatus || isActive}
                aria-pressed={isActive}
                className={`inline-flex h-8 items-center px-3 text-xs font-medium transition-colors first:rounded-l-md last:rounded-r-md focus-visible:outline-none ${
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "hover:bg-muted disabled:cursor-not-allowed"
                }`}
              >
                {s.label}
              </button>
            );
          })}
        </div>
      </div>

      {/* Management links */}
      <h2 className="mb-3 text-base font-medium">Verwaltung</h2>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        {[
          { to: `/organizer/conferences/${id}/rooms`, label: "Räume", desc: "Räume anlegen und verwalten" },
          { to: `/organizer/conferences/${id}/talk-types`, label: "Talk-Typen", desc: "Formate definieren" },
          { to: `/organizer/conferences/${id}/proposals`, label: "Einreichungen", desc: "Talks annehmen oder ablehnen" },
          { to: `/organizer/conferences/${id}/schedule`, label: "Zeitplan", desc: "Talks per Drag & Drop einplanen" },
        ].map((item) => (
          <Link
            key={item.to}
            to={item.to}
            className="border-border hover:border-primary/50 hover:shadow-sm rounded-lg border p-4 transition-all"
          >
            <p className="font-medium">{item.label}</p>
            <p className="text-muted-foreground mt-1 text-xs">{item.desc}</p>
          </Link>
        ))}
      </div>
    </PageLayout>
  );
}
