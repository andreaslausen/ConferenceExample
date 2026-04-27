import { useState, useEffect, type FormEvent } from "react";
import { useParams } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { ConfirmDialog } from "../shared/components/Dialog";
import { useToast } from "../shared/components/Toast";

type TalkType = components["schemas"]["GetConferenceTalkTypesDto"];

export default function OrganizerTalkTypesPage() {
  const { id } = useParams<{ id: string }>();
  const { toast } = useToast();

  const [talkTypes, setTalkTypes] = useState<TalkType[]>([]);
  const [loading, setLoading] = useState(true);
  const [newName, setNewName] = useState("");
  const [newDuration, setNewDuration] = useState(45);
  const [adding, setAdding] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<TalkType | null>(null);

  useEffect(() => {
    if (!id) return;
    apiClient
      .GET("/api/Conferences/{id}/talk-types", { params: { path: { id } } })
      .then(({ data }) => {
        setTalkTypes(data ?? []);
        setLoading(false);
      });
  }, [id]);

  async function handleAdd(e: FormEvent) {
    e.preventDefault();
    if (!id || !newName.trim()) return;
    setAdding(true);
    const { data, error } = await apiClient.POST(
      "/api/Conferences/{id}/talk-types",
      {
        params: { path: { id } },
        body: { name: newName.trim(), durationInMinutes: Number(newDuration) },
      },
    );
    setAdding(false);
    if (error || !data) {
      toast({ title: "Talk-Typ konnte nicht angelegt werden.", variant: "destructive" });
      return;
    }
    setTalkTypes((prev) => [
      ...prev,
      { id: data.talkTypeId, name: data.name, durationInMinutes: data.durationInMinutes },
    ]);
    setNewName("");
    setNewDuration(45);
    toast({ title: `Talk-Typ „${data.name}" angelegt.` });
  }

  async function handleDelete(tt: TalkType) {
    if (!id) return;
    const { error } = await apiClient.DELETE(
      "/api/Conferences/{conferenceId}/talk-types/{talkTypeId}",
      { params: { path: { conferenceId: id, talkTypeId: tt.id } } },
    );
    if (error) {
      toast({ title: "Talk-Typ konnte nicht gelöscht werden.", variant: "destructive" });
      return;
    }
    setTalkTypes((prev) => prev.filter((t) => t.id !== tt.id));
    toast({ title: `Talk-Typ „${tt.name}" gelöscht.` });
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Konferenzen", to: "/organizer/conferences" },
          { label: "Konferenz", to: `/organizer/conferences/${id}` },
          { label: "Talk-Typen" },
        ]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Talk-Typen</h1>

      {loading ? (
        <div className="space-y-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      ) : (
        <div className="max-w-lg space-y-4">
          {talkTypes.length === 0 ? (
            <p className="text-muted-foreground text-sm">
              Noch keine Talk-Typen definiert.
            </p>
          ) : (
            <ul className="divide-y border-border rounded-lg border" role="list">
              {talkTypes.map((tt) => (
                <li
                  key={tt.id}
                  className="flex items-center justify-between px-4 py-3"
                >
                  <span className="text-sm">{tt.name} — {tt.durationInMinutes} Min.</span>
                  <button
                    onClick={() => setDeleteTarget(tt)}
                    aria-label={`Talk-Typ ${tt.name} löschen`}
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
              placeholder="z. B. Keynote, Workshop…"
              required
              className="border-input bg-background focus-visible:ring-ring flex h-10 flex-1 rounded-md border px-3 text-sm focus-visible:ring-2 focus-visible:outline-none"
              aria-label="Name des neuen Talk-Typs"
            />
            <input
              type="number"
              min={1}
              value={newDuration}
              onChange={(e) => setNewDuration(Number(e.target.value))}
              required
              className="border-input bg-background focus-visible:ring-ring flex h-10 w-24 rounded-md border px-3 text-sm focus-visible:ring-2 focus-visible:outline-none"
              aria-label="Dauer in Minuten"
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
        title={`Talk-Typ „${deleteTarget?.name}" löschen?`}
        description="Diese Aktion kann nicht rückgängig gemacht werden."
        onConfirm={() => deleteTarget && handleDelete(deleteTarget)}
        confirmLabel="Löschen"
        variant="destructive"
      />
    </PageLayout>
  );
}
