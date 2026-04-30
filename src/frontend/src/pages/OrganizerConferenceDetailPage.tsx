import { useState, useEffect, type FormEvent, type KeyboardEvent } from "react";
import { useParams, Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type ConferenceDetail = components["schemas"]["GetConferenceByIdDto"];

const STATUSES = [
  { value: 0, label: "Entwurf", backendName: "Draft" },
  { value: 1, label: "Call for Speakers", backendName: "CallForSpeakers" },
  { value: 2, label: "Call for Speakers geschlossen", backendName: "CallForSpeakersClosed" },
  { value: 3, label: "Programm veröffentlicht", backendName: "ProgramPublished" },
];

const STATUS_LABELS: Record<string, string> = {
  Draft: "Entwurf",
  CallForSpeakers: "Call for Speakers",
  CallForSpeakersClosed: "Call for Speakers geschlossen",
  ProgramPublished: "Programm veröffentlicht",
};

const STATUS_ORDER = {
  Draft: 0,
  CallForSpeakers: 1,
  CallForSpeakersClosed: 2,
  ProgramPublished: 3,
} as const;

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
    const { error, response } = await apiClient.PUT("/api/Conferences/{id}/status", {
      params: { path: { id } },
      body: { status: statusValue },
    });
    setChangingStatus(false);
    if (error) {
      // Try to extract error message from response
      let errorMessage = "Statusänderung fehlgeschlagen.";
      if (response && !response.ok) {
        try {
          const errorData = await response.text();
          if (errorData) {
            errorMessage = errorData;
          }
        } catch {
          // Ignore parse errors
        }
      }
      toast({
        title: "Statusänderung fehlgeschlagen",
        description: errorMessage,
        variant: "destructive"
      });
      return;
    }
    const newStatus = STATUSES.find((s) => s.value === statusValue);
    const newStatusLabel = newStatus?.label ?? "";
    const newStatusName = newStatus?.backendName ?? "";
    setConference((c) => c ? { ...c, status: newStatusName } : c);
    toast({ title: `Status auf "${newStatusLabel}" gesetzt.` });
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
    (s) => s.backendName === conference.status,
  );

  // Check if conference is editable (only Draft status can be edited)
  const isEditable =
    conference?.status &&
    STATUS_ORDER[conference.status as keyof typeof STATUS_ORDER] <
    STATUS_ORDER.CallForSpeakers;

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
            {isEditable && (
              <button
                onClick={startRenaming}
                aria-label="Name bearbeiten"
                className="text-muted-foreground hover:text-foreground text-sm"
              >
                ✎
              </button>
            )}
          </>
        )}
      </div>

      <div className="mb-4 flex items-center justify-between">
        <p className="text-muted-foreground text-sm">
          {formatDate(conference.startDate)} – {formatDate(conference.endDate)} ·{" "}
          {conference.locationName}, {conference.city}
        </p>
        {isEditable ? (
          <Link
            to={`/organizer/conferences/${id}/edit`}
            className="bg-secondary text-secondary-foreground hover:bg-secondary/80 inline-flex h-9 items-center rounded-md px-3 text-sm font-medium"
          >
            Bearbeiten
          </Link>
        ) : (
          <span className="text-muted-foreground inline-flex h-9 items-center px-3 text-sm">
            Nicht bearbeitbar
          </span>
        )}
      </div>

      {/* Status change */}
      <div className="mb-8">
        <div className="mb-3 flex items-center gap-3">
          <span className="text-sm font-medium">Aktueller Status:</span>
          <span className="rounded-md bg-muted px-3 py-1.5 text-sm font-medium">
            {STATUS_LABELS[conference.status] ?? conference.status}
          </span>
        </div>
        <div className="flex items-center gap-3">
          {/* Backward button */}
          {currentStatusIndex > 0 && (
            <>
              <button
                onClick={() => handleStatusChange(STATUSES[currentStatusIndex - 1].value)}
                disabled={changingStatus}
                className="border-input bg-background hover:bg-accent hover:text-accent-foreground inline-flex h-9 items-center rounded-md border px-4 text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {changingStatus ? "Wird geändert..." : `← Zurück zu "${STATUSES[currentStatusIndex - 1].label}"`}
              </button>
            </>
          )}
          {/* Forward button */}
          {currentStatusIndex < STATUSES.length - 1 && (
            <>
              <button
                onClick={() => handleStatusChange(STATUSES[currentStatusIndex + 1].value)}
                disabled={changingStatus}
                className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-4 text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {changingStatus ? "Wird geändert..." : `Weiter zu "${STATUSES[currentStatusIndex + 1].label}" →`}
              </button>
            </>
          )}
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
