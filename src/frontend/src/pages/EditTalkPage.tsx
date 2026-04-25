import { useState, useEffect, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "../shared/api/client";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

export default function EditTalkPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [notFound, setNotFound] = useState(false);

  const [title, setTitle] = useState("");
  const [abstract, setAbstract] = useState("");
  const [tagsInput, setTagsInput] = useState("");

  useEffect(() => {
    if (!id) return;
    apiClient.GET("/api/Talks/{id}", { params: { path: { id } } }).then(({ data }) => {
      if (!data) {
        setNotFound(true);
      } else {
        setTitle(data.title);
        setAbstract(data.abstract);
        setTagsInput(data.tags.join(", "));
      }
      setLoading(false);
    });
  }, [id]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!id) return;
    setSaving(true);

    const tags = tagsInput
      .split(",")
      .map((t) => t.trim())
      .filter(Boolean);

    const { error } = await apiClient.PUT("/api/Talks/{id}", {
      params: { path: { id } },
      body: { title, abstract, tags },
    });

    if (error) {
      toast({ title: "Fehler beim Speichern.", variant: "destructive" });
      setSaving(false);
      return;
    }
    toast({ title: "Talk aktualisiert." });
    navigate("/my-talks");
  }

  if (loading) {
    return (
      <PageLayout>
        <div className="max-w-lg space-y-4">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-32 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      </PageLayout>
    );
  }

  if (notFound) {
    return (
      <PageLayout>
        <p className="text-destructive text-sm">Talk nicht gefunden.</p>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Meine Talks", to: "/my-talks" },
          { label: "Talk bearbeiten" },
        ]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Talk bearbeiten</h1>

      <form onSubmit={handleSubmit} className="max-w-lg space-y-4">
        <div className="space-y-1.5">
          <label htmlFor="title" className="text-sm font-medium">
            Titel <span aria-hidden="true" className="text-destructive">*</span>
          </label>
          <input
            id="title"
            required
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="border-input bg-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
            aria-required="true"
          />
        </div>

        <div className="space-y-1.5">
          <label htmlFor="abstract" className="text-sm font-medium">
            Abstract <span aria-hidden="true" className="text-destructive">*</span>
          </label>
          <textarea
            id="abstract"
            required
            rows={5}
            value={abstract}
            onChange={(e) => setAbstract(e.target.value)}
            className="border-input bg-background focus-visible:ring-ring flex w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
            aria-required="true"
          />
        </div>

        <div className="space-y-1.5">
          <label htmlFor="tags" className="text-sm font-medium">
            Tags{" "}
            <span className="text-muted-foreground font-normal">
              (kommagetrennt)
            </span>
          </label>
          <input
            id="tags"
            value={tagsInput}
            onChange={(e) => setTagsInput(e.target.value)}
            className="border-input bg-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
          />
        </div>

        <div className="flex gap-2 pt-2">
          <button
            type="submit"
            disabled={saving}
            className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-10 items-center rounded-md px-4 text-sm font-medium disabled:pointer-events-none disabled:opacity-50"
          >
            {saving ? "Speichern…" : "Speichern"}
          </button>
          <button
            type="button"
            onClick={() => navigate("/my-talks")}
            className="border-input hover:bg-accent inline-flex h-10 items-center rounded-md border px-4 text-sm"
          >
            Abbrechen
          </button>
        </div>
      </form>
    </PageLayout>
  );
}
