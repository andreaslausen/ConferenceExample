import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";

type ConferenceDetail = components["schemas"]["GetConferenceByIdDto"];
type ProgramEntry = components["schemas"]["GetConferenceProgramDto"];

interface TimeSlot {
  key: string;
  start: string;
  end: string;
}

interface Room {
  id: string;
  name: string;
}

function formatDate(iso: string): string {
  return new Intl.DateTimeFormat("de-DE", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(iso));
}

function formatTime(iso: string): string {
  return new Intl.DateTimeFormat("de-DE", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(iso));
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

function ConferenceHeaderSkeleton() {
  return (
    <div className="mb-8 space-y-3">
      <Skeleton className="h-8 w-64" />
      <Skeleton className="h-4 w-48" />
      <Skeleton className="h-4 w-40" />
    </div>
  );
}

function ProgramGridSkeleton() {
  return (
    <div className="overflow-x-auto">
      <div className="min-w-[600px] space-y-2">
        <div className="grid gap-2" style={{ gridTemplateColumns: "120px repeat(3, 1fr)" }}>
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-10 rounded-md" />
          ))}
        </div>
        {Array.from({ length: 3 }).map((_, row) => (
          <div
            key={row}
            className="grid gap-2"
            style={{ gridTemplateColumns: "120px repeat(3, 1fr)" }}
          >
            {Array.from({ length: 4 }).map((_, col) => (
              <Skeleton key={col} className="h-20 rounded-md" />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}

function buildGrid(entries: ProgramEntry[]): {
  slots: TimeSlot[];
  rooms: Room[];
  lookup: Map<string, ProgramEntry>;
} {
  const scheduled = entries.filter(
    (e) => e.slotStart !== null && e.roomId !== null,
  );

  const slotMap = new Map<string, TimeSlot>();
  const roomMap = new Map<string, Room>();

  for (const e of scheduled) {
    const key = `${e.slotStart}|${e.slotEnd}`;
    if (!slotMap.has(key)) {
      slotMap.set(key, { key, start: e.slotStart!, end: e.slotEnd! });
    }
    if (!roomMap.has(e.roomId!)) {
      roomMap.set(e.roomId!, { id: e.roomId!, name: e.roomName ?? "" });
    }
  }

  const slots = [...slotMap.values()].sort(
    (a, b) => new Date(a.start).getTime() - new Date(b.start).getTime(),
  );
  const rooms = [...roomMap.values()].sort((a, b) =>
    a.name.localeCompare(b.name),
  );

  const lookup = new Map<string, ProgramEntry>();
  for (const e of scheduled) {
    const key = `${e.slotStart}|${e.slotEnd}|${e.roomId}`;
    lookup.set(key, e);
  }

  return { slots, rooms, lookup };
}

export default function ConferenceProgramPage() {
  const { id } = useParams<{ id: string }>();
  const [conference, setConference] = useState<ConferenceDetail | null>(null);
  const [program, setProgram] = useState<ProgramEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    setLoading(true);
    setError(null);

    Promise.all([
      apiClient.GET("/api/Conferences/{id}", { params: { path: { id } } }),
      apiClient.GET("/api/Conferences/{id}/program", {
        params: { path: { id } },
      }),
    ]).then(([confResult, programResult]) => {
      if (cancelled) return;
      if (confResult.error || !confResult.data) {
        setError("Konferenz konnte nicht geladen werden.");
        setLoading(false);
        return;
      }
      setConference(confResult.data);
      setProgram(programResult.data ?? []);
      setLoading(false);
    });

    return () => {
      cancelled = true;
    };
  }, [id]);

  if (loading) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8">
        <ConferenceHeaderSkeleton />
        <ProgramGridSkeleton />
      </div>
    );
  }

  if (error || !conference) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8">
        <p role="alert" className="text-destructive text-sm">
          {error ?? "Konferenz nicht gefunden."}
        </p>
      </div>
    );
  }

  const { slots, rooms, lookup } = buildGrid(program);
  const hasScheduled = slots.length > 0;

  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      {/* Conference header */}
      <div className="mb-8">
        <div className="mb-2 flex flex-wrap items-center gap-3">
          <h1 className="text-2xl font-semibold">{conference.name}</h1>
          <StatusBadge status={conference.status} />
        </div>
        <p className="text-muted-foreground text-sm">
          {formatDate(conference.startDate)} – {formatDate(conference.endDate)}
        </p>
        <p className="text-muted-foreground text-sm">
          {conference.locationName}, {conference.city}, {conference.country}
        </p>
      </div>

      <h2 className="mb-4 text-lg font-medium">Programm</h2>

      {!hasScheduled ? (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <p className="text-muted-foreground">
            Für diese Konferenz ist noch kein Programm veröffentlicht.
          </p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full border-separate border-spacing-1">
            <thead>
              <tr>
                <th className="text-muted-foreground w-28 px-2 py-2 text-left text-xs font-medium">
                  Zeit
                </th>
                {rooms.map((room) => (
                  <th
                    key={room.id}
                    className="bg-muted rounded-md px-3 py-2 text-left text-xs font-medium"
                  >
                    {room.name}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {slots.map((slot) => (
                <tr key={slot.key}>
                  <td className="text-muted-foreground w-28 px-2 py-2 align-top text-xs">
                    <div>{formatTime(slot.start)}</div>
                    <div>{formatTime(slot.end)}</div>
                  </td>
                  {rooms.map((room) => {
                    const cellKey = `${slot.start}|${slot.end}|${room.id}`;
                    const entry = lookup.get(cellKey);
                    return (
                      <td key={room.id} className="align-top">
                        {entry ? (
                          <Link
                            to={`/talks/${entry.talkId}`}
                            className="bg-card border-border hover:border-primary/50 hover:bg-accent block h-full rounded-md border p-3 transition-colors"
                          >
                            <p className="text-sm font-medium leading-snug">
                              {entry.talkTitle}
                            </p>
                            <p className="text-muted-foreground mt-1 text-xs">
                              {entry.speakerName}
                            </p>
                          </Link>
                        ) : (
                          <div className="bg-muted/30 h-full min-h-[4rem] rounded-md" />
                        )}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
