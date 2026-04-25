import { useState, useEffect, useCallback } from "react";
import { useParams } from "react-router-dom";
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  useSensor,
  useSensors,
  type DragStartEvent,
  type DragEndEvent,
} from "@dnd-kit/core";
import { useDraggable, useDroppable } from "@dnd-kit/core";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type ScheduledTalk = components["schemas"]["GetConferenceScheduleDto"];
type Room = components["schemas"]["GetConferenceRoomsDto"];

interface TimeSlot {
  key: string;
  start: string;
  end: string;
}

function formatTime(iso: string): string {
  return new Intl.DateTimeFormat("de-DE", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(iso));
}

function buildTimeSlots(talks: ScheduledTalk[]): TimeSlot[] {
  const map = new Map<string, TimeSlot>();
  for (const t of talks) {
    if (t.slotStart && t.slotEnd) {
      const key = `${t.slotStart}|${t.slotEnd}`;
      if (!map.has(key)) {
        map.set(key, { key, start: t.slotStart, end: t.slotEnd });
      }
    }
  }
  return [...map.values()].sort(
    (a, b) => new Date(a.start).getTime() - new Date(b.start).getTime(),
  );
}

function buildLookup(talks: ScheduledTalk[]): Map<string, ScheduledTalk> {
  const m = new Map<string, ScheduledTalk>();
  for (const t of talks) {
    if (t.slotStart && t.slotEnd && t.roomId) {
      m.set(`${t.slotStart}|${t.slotEnd}|${t.roomId}`, t);
    }
  }
  return m;
}

/* ── Draggable Talk Card ─────────────────────────────────────── */
function TalkCard({ talk, isDragging = false }: { talk: ScheduledTalk; isDragging?: boolean }) {
  const STATUS_LABELS: Record<string, string> = {
    Pending: "Ausstehend",
    Accepted: "Angenommen",
    Rejected: "Abgelehnt",
  };
  return (
    <div
      className={`rounded-md border p-2 text-xs ${
        isDragging ? "opacity-50" : "bg-card border-border"
      }`}
    >
      <p className="font-medium leading-tight">{talk.title}</p>
      {!isDragging && (
        <p className="text-muted-foreground mt-0.5">
          {STATUS_LABELS[talk.status] ?? talk.status}
        </p>
      )}
    </div>
  );
}

/* ── Draggable sidebar item ──────────────────────────────────── */
function DraggableTalk({ talk }: { talk: ScheduledTalk }) {
  const { attributes, listeners, setNodeRef, isDragging } = useDraggable({
    id: talk.id,
    data: { talk },
  });
  return (
    <div
      ref={setNodeRef}
      {...listeners}
      {...attributes}
      className={`cursor-grab active:cursor-grabbing ${isDragging ? "opacity-40" : ""}`}
      role="button"
      aria-label={`Talk ${talk.title} verschieben`}
    >
      <TalkCard talk={talk} />
    </div>
  );
}

/* ── Drop Cell ───────────────────────────────────────────────── */
function DropCell({
  slot,
  room,
  existing,
}: {
  slot: TimeSlot;
  room: Room;
  existing: ScheduledTalk | undefined;
}) {
  const dropId = `${slot.key}|${room.id}`;
  const { setNodeRef, isOver } = useDroppable({ id: dropId, data: { slot, room } });

  return (
    <td
      ref={setNodeRef}
      className={`min-w-[140px] border-border border p-1 align-top transition-colors ${
        isOver ? "bg-primary/10" : "bg-background"
      }`}
    >
      {existing ? (
        <DraggableTalk talk={existing} />
      ) : (
        <div className="min-h-[3.5rem]" />
      )}
    </td>
  );
}

export default function OrganizerSchedulePage() {
  const { id } = useParams<{ id: string }>();
  const { toast } = useToast();

  const [rooms, setRooms] = useState<Room[]>([]);
  const [talks, setTalks] = useState<ScheduledTalk[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTalk, setActiveTalk] = useState<ScheduledTalk | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
  );

  const load = useCallback(async () => {
    if (!id) return;
    const [roomsRes, talksRes] = await Promise.all([
      apiClient.GET("/api/Conferences/{id}/rooms", { params: { path: { id } } }),
      apiClient.GET("/api/Conferences/{id}/schedule", { params: { path: { id } } }),
    ]);
    setRooms(roomsRes.data ?? []);
    setTalks(talksRes.data ?? []);
    setLoading(false);
  }, [id]);

  useEffect(() => {
    load();
  }, [load]);

  function handleDragStart(event: DragStartEvent) {
    const talk = talks.find((t) => t.id === event.active.id);
    setActiveTalk(talk ?? null);
  }

  async function handleDragEnd(event: DragEndEvent) {
    setActiveTalk(null);
    const { active, over } = event;
    if (!over || !id) return;

    const talk = talks.find((t) => t.id === active.id);
    if (!talk) return;

    const dropData = over.data.current as { slot: TimeSlot; room: Room } | undefined;
    if (!dropData) return;

    const { slot, room } = dropData;

    const prevSlotStart = talk.slotStart;
    const prevSlotEnd = talk.slotEnd;
    const prevRoomId = talk.roomId;

    const slotChanged =
      talk.slotStart !== slot.start || talk.slotEnd !== slot.end;
    const roomChanged = talk.roomId !== room.id;

    if (!slotChanged && !roomChanged) return;

    // Optimistic update
    setTalks((prev) =>
      prev.map((t) =>
        t.id === talk.id
          ? { ...t, slotStart: slot.start, slotEnd: slot.end, roomId: room.id, roomName: room.name }
          : t,
      ),
    );

    try {
      if (slotChanged) {
        const { error } = await apiClient.PUT(
          "/api/Conferences/{conferenceId}/talks/{talkId}/schedule",
          {
            params: { path: { conferenceId: id, talkId: talk.id } },
            body: { start: slot.start, end: slot.end },
          },
        );
        if (error) throw new Error("schedule");
      }
      if (roomChanged) {
        const { error } = await apiClient.PUT(
          "/api/Conferences/{conferenceId}/talks/{talkId}/room",
          {
            params: { path: { conferenceId: id, talkId: talk.id } },
            body: { roomId: room.id },
          },
        );
        if (error) throw new Error("room");
      }
      toast({ title: "Talk eingeplant." });
    } catch {
      // Roll back
      setTalks((prev) =>
        prev.map((t) =>
          t.id === talk.id
            ? { ...t, slotStart: prevSlotStart, slotEnd: prevSlotEnd, roomId: prevRoomId }
            : t,
        ),
      );
      toast({ title: "Einplanen fehlgeschlagen.", variant: "destructive" });
    }
  }

  const slots = buildTimeSlots(talks);
  const lookup = buildLookup(talks);
  const unscheduled = talks.filter((t) => !t.slotStart && t.status === "Accepted");
  const hasSlots = slots.length > 0 || unscheduled.length > 0;

  if (loading) {
    return (
      <PageLayout>
        <Skeleton className="mb-4 h-8 w-48" />
        <div className="flex gap-4">
          <Skeleton className="h-64 w-48 shrink-0" />
          <Skeleton className="h-64 flex-1" />
        </div>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Konferenzen", to: "/organizer/conferences" },
          { label: "Konferenz", to: `/organizer/conferences/${id}` },
          { label: "Zeitplan" },
        ]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Zeitplan-Editor</h1>

      {rooms.length === 0 && (
        <p className="text-muted-foreground mb-4 text-sm">
          Es sind noch keine Räume angelegt. Lege zuerst{" "}
          <a
            href={`/organizer/conferences/${id}/rooms`}
            className="text-primary underline"
          >
            Räume
          </a>{" "}
          an.
        </p>
      )}

      {!hasSlots && rooms.length > 0 && (
        <p className="text-muted-foreground mb-4 text-sm">
          Noch keine angenommenen Talks zum Einplanen.
        </p>
      )}

      <DndContext
        sensors={sensors}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <div className="flex gap-4">
          {/* Sidebar: unscheduled accepted talks */}
          {unscheduled.length > 0 && (
            <aside className="w-48 shrink-0">
              <p className="text-muted-foreground mb-2 text-xs font-medium uppercase tracking-wide">
                Nicht eingeplant
              </p>
              <div className="space-y-2">
                {unscheduled.map((t) => (
                  <DraggableTalk key={t.id} talk={t} />
                ))}
              </div>
            </aside>
          )}

          {/* Grid */}
          {rooms.length > 0 && slots.length > 0 && (
            <div className="flex-1 overflow-x-auto">
              <table className="border-collapse text-sm">
                <thead>
                  <tr>
                    <th className="border-border w-24 border bg-muted/50 px-2 py-2 text-left text-xs font-medium">
                      Zeit
                    </th>
                    {rooms.map((room) => (
                      <th
                        key={room.id}
                        className="border-border min-w-[140px] border bg-muted/50 px-2 py-2 text-left text-xs font-medium"
                      >
                        {room.name}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {slots.map((slot) => (
                    <tr key={slot.key}>
                      <td className="border-border w-24 border px-2 py-2 align-top text-xs text-muted-foreground">
                        <div>{formatTime(slot.start)}</div>
                        <div>{formatTime(slot.end)}</div>
                      </td>
                      {rooms.map((room) => {
                        const cellKey = `${slot.start}|${slot.end}|${room.id}`;
                        const existing = lookup.get(cellKey);
                        return (
                          <DropCell
                            key={room.id}
                            slot={slot}
                            room={room}
                            existing={existing}
                          />
                        );
                      })}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <DragOverlay>
          {activeTalk && <TalkCard talk={activeTalk} isDragging />}
        </DragOverlay>
      </DndContext>
    </PageLayout>
  );
}
