import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../shared/api/client";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

/**
 * Creating a talk is independent of any conference — a talk exists on its own, and is submitted
 * somewhere later from the talk list.
 */
export default function NewTalkPage() {
  const navigate = useNavigate();
  const { toast } = useToast();

  const [submitting, setSubmitting] = useState(false);
  const [title, setTitle] = useState("");
  const [abstract, setAbstract] = useState("");
  const [tagsInput, setTagsInput] = useState("");

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);

    const tags = tagsInput
      .split(",")
      .map((t) => t.trim())
      .filter(Boolean);

    const { error } = await apiClient.POST("/api/Talks", {
      body: { title, abstract, tags },
    });

    if (error) {
      toast({
        title: "Talk konnte nicht angelegt werden.",
        description: "Hast du schon ein Speaker-Profil?",
        variant: "destructive",
      });
      setSubmitting(false);
      return;
    }

    toast({ title: "Talk angelegt." });
    navigate("/my-talks");
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[{ label: "Meine Talks", to: "/my-talks" }, { label: "Neuer Talk" }]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Neuer Talk</h1>

      <form onSubmit={handleSubmit} className="max-w-lg space-y-4">
        <div>
          <label htmlFor="title" className="mb-1.5 block text-sm font-medium">
            Titel
          </label>
          <input
            id="title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            maxLength={100}
            className="border-input bg-background focus-visible:ring-ring h-9 w-full rounded-md border px-3 text-sm focus-visible:ring-2 focus-visible:outline-none"
          />
        </div>

        <div>
          <label htmlFor="abstract" className="mb-1.5 block text-sm font-medium">
            Abstract
          </label>
          <textarea
            id="abstract"
            value={abstract}
            onChange={(e) => setAbstract(e.target.value)}
            required
            maxLength={1000}
            rows={8}
            className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
          />
        </div>

        <div>
          <label htmlFor="tags" className="mb-1.5 block text-sm font-medium">
            Tags
          </label>
          <input
            id="tags"
            value={tagsInput}
            onChange={(e) => setTagsInput(e.target.value)}
            placeholder="Architecture, CQRS"
            className="border-input bg-background focus-visible:ring-ring h-9 w-full rounded-md border px-3 text-sm focus-visible:ring-2 focus-visible:outline-none"
          />
          <p className="text-muted-foreground mt-1 text-xs">Kommagetrennt, optional.</p>
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-4 text-sm font-medium disabled:opacity-50"
        >
          {submitting ? "Wird angelegt…" : "Talk anlegen"}
        </button>
      </form>
    </PageLayout>
  );
}
