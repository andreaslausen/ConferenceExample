import { useState, useEffect, type FormEvent } from "react";
import { useParams } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { ConfirmDialog } from "../shared/components/Dialog";
import { useToast } from "../shared/components/Toast";

type Room = components["schemas"]["GetConferenceRoomsDto"];

export default function OrganizerRoomsPage() {
  const { id } = useParams<{ id: string }>();
  const { toast } = useToast();

  const [rooms, setRooms] = useState<Room[]>([]);
  const [loading, setLoading] = useState(true);
  const [newName, setNewName] = useState("");
  const [adding, setAdding] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<Room | null>(null);

  useEffect(() => {
    if (!id) return;
    apiClient
      .GET("/api/Conferences/{id}/rooms", { params: { path: { id } } })
      .then(({ data }) => {
        setRooms(data ?? []);
        setLoading(false);
      });
  }, [id]);

  async function handleAdd(e: FormEvent) {
    e.preventDefault();
    if (!id || !newName.trim()) return;
    setAdding(true);
    const { data, error } = await apiClient.POST("/api/Conferences/{id}/rooms", {
      params: { path: { id } },
      body: { name: newName.trim() },
    });
    setAdding(false);
    if (error || !data) {
      toast({ title: "Raum konnte nicht angelegt werden.", variant: "destructive" });
      return;
    }
    setRooms((prev) => [...prev, { id: data.roomId, name: data.name }]);
    setNewName("");
    toast({ title: `Raum „${data.name}" angelegt.` });
  }

  async function handleDelete(room: Room) {
    if (!id) return;
    const { error } = await apiClient.DELETE(
      "/api/Conferences/{conferenceId}/rooms/{roomId}",
      { params: { path: { conferenceId: id, roomId: room.id } } },
    );
    if (error) {
      toast({ title: "Raum konnte nicht gelöscht werden.", variant: "destructive" });
      return;
    }
    setRooms((prev) => prev.filter((r) => r.id !== room.id));
    toast({ title: `Raum „${room.name}" gelöscht.` });
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Konferenzen", to: "/organizer/conferences" },
          { label: "Konferenz", to: `/organizer/conferences/${id}` },
          { label: "Räume" },
        ]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Räume</h1>

      {loading ? (
        <div className="space-y-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      ) : (
        <div className="max-w-lg space-y-4">
          {rooms.length === 0 ? (
            <p className="text-muted-foreground text-sm">Noch keine Räume angelegt.</p>
          ) : (
            <ul className="divide-y border-border rounded-lg border" role="list">
              {rooms.map((room) => (
                <li key={room.id} className="flex items-center justify-between px-4 py-3">
                  <span className="text-sm">{room.name}</span>
                  <button
                    onClick={() => setDeleteTarget(room)}
                    aria-label={`Raum ${room.name} löschen`}
                    className="text-destructive hover:opacity-70 text-xs"
                  >
                    Löschen
                  </button>
                </li>
              ))}
            </ul>
          )}

          <form onSubmit={handleAdd} className="flex gap-2">
            <input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder="Neuer Raum…"
              required
              className="border-input bg-background focus-visible:ring-ring flex h-10 flex-1 rounded-md border px-3 text-sm focus-visible:ring-2 focus-visible:outline-none"
              aria-label="Name des neuen Raums"
            />
            <button
              type="submit"
              disabled={adding || !newName.trim()}
              className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-10 items-center rounded-md px-3 text-sm font-medium disabled:pointer-events-none disabled:opacity-50"
            >
              {adding ? "…" : "Hinzufügen"}
            </button>
          </form>
        </div>
      )}

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        title={`Raum „${deleteTarget?.name}" löschen?`}
        description="Diese Aktion kann nicht rückgängig gemacht werden."
        onConfirm={() => deleteTarget && handleDelete(deleteTarget)}
        confirmLabel="Löschen"
        variant="destructive"
      />
    </PageLayout>
  );
}
