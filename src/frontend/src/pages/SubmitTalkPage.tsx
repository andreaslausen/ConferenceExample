import { useState, useEffect, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type Conference = components["schemas"]["GetAllConferencesDto"];
type TalkType = components["schemas"]["GetConferenceTalkTypesDto"];

export default function SubmitTalkPage() {
  const navigate = useNavigate();
  const { toast } = useToast();

  const [conferences, setConferences] = useState<Conference[]>([]);
  const [talkTypes, setTalkTypes] = useState<TalkType[]>([]);
  const [loadingConferences, setLoadingConferences] = useState(true);
  const [loadingTalkTypes, setLoadingTalkTypes] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const [title, setTitle] = useState("");
  const [abstract, setAbstract] = useState("");
  const [tagsInput, setTagsInput] = useState("");
  const [conferenceId, setConferenceId] = useState("");
  const [talkTypeId, setTalkTypeId] = useState("");

  useEffect(() => {
    apiClient
      .GET("/api/Conferences", { params: { query: { page: 1, pageSize: 100 } } })
      .then(({ data }) => {
        setConferences(data?.items ?? []);
        setLoadingConferences(false);
      });
  }, []);

  useEffect(() => {
    if (!conferenceId) {
      setTalkTypes([]);
      setTalkTypeId("");
      return;
    }
    setLoadingTalkTypes(true);
    setTalkTypeId("");
    apiClient
      .GET("/api/Conferences/{id}/talk-types", {
        params: { path: { id: conferenceId } },
      })
      .then(({ data }) => {
        setTalkTypes(data ?? []);
        setLoadingTalkTypes(false);
      });
  }, [conferenceId]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!conferenceId || !talkTypeId) {
      toast({ title: "Bitte Konferenz und Talk-Typ auswählen.", variant: "destructive" });
      return;
    }
    setSubmitting(true);
    const tags = tagsInput
      .split(",")
      .map((t) => t.trim())
      .filter(Boolean);

    const { error } = await apiClient.POST("/api/Talks", {
      body: { title, abstract, conferenceId, tags, talkTypeId },
    });

    if (error) {
      toast({ title: "Einreichung fehlgeschlagen.", variant: "destructive" });
      setSubmitting(false);
      return;
    }
    toast({ title: "Talk erfolgreich eingereicht." });
    navigate("/my-talks");
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Meine Talks", to: "/my-talks" },
          { label: "Talk einreichen" },
        ]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Talk einreichen</h1>

      {loadingConferences ? (
        <div className="max-w-lg space-y-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-10 w-full" />
          ))}
        </div>
      ) : (
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
              placeholder="React, TypeScript, DDD"
              className="border-input bg-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
            />
          </div>

          <div className="space-y-1.5">
            <label htmlFor="conference" className="text-sm font-medium">
              Konferenz <span aria-hidden="true" className="text-destructive">*</span>
            </label>
            <select
              id="conference"
              required
              value={conferenceId}
              onChange={(e) => setConferenceId(e.target.value)}
              className="border-input bg-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
              aria-required="true"
            >
              <option value="">Konferenz auswählen…</option>
              {conferences.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1.5">
            <label htmlFor="talkType" className="text-sm font-medium">
              Talk-Typ <span aria-hidden="true" className="text-destructive">*</span>
            </label>
            {loadingTalkTypes ? (
              <Skeleton className="h-10 w-full" />
            ) : (
              <select
                id="talkType"
                required
                value={talkTypeId}
                onChange={(e) => setTalkTypeId(e.target.value)}
                disabled={!conferenceId || talkTypes.length === 0}
                className="border-input bg-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50"
                aria-required="true"
              >
                <option value="">
                  {!conferenceId
                    ? "Erst Konferenz auswählen"
                    : talkTypes.length === 0
                      ? "Keine Talk-Typen verfügbar"
                      : "Talk-Typ auswählen…"}
                </option>
                {talkTypes.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </select>
            )}
          </div>

          <div className="flex gap-2 pt-2">
            <button
              type="submit"
              disabled={submitting}
              className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-10 items-center rounded-md px-4 text-sm font-medium disabled:pointer-events-none disabled:opacity-50"
            >
              {submitting ? "Einreichen…" : "Einreichen"}
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
      )}
    </PageLayout>
  );
}
