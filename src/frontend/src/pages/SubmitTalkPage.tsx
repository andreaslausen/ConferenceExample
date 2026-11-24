import { useState, useEffect, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type Conference = components["schemas"]["GetAllConferencesDto"];
type TalkType = components["schemas"]["GetConferenceTalkTypesDto"];

/**
 * Submits an existing talk to a conference. The talk type belongs to the conference, so it can
 * only be picked once a conference is chosen.
 */
export default function SubmitTalkPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();

  const [conferences, setConferences] = useState<Conference[]>([]);
  const [talkTypes, setTalkTypes] = useState<TalkType[]>([]);
  const [loadingConferences, setLoadingConferences] = useState(true);
  const [loadingTalkTypes, setLoadingTalkTypes] = useState(false);
  const [submitting, setSubmitting] = useState(false);

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
    if (!id || !conferenceId || !talkTypeId) {
      toast({ title: "Bitte Konferenz und Talk-Typ auswählen.", variant: "destructive" });
      return;
    }
    setSubmitting(true);

    const { error } = await apiClient.POST("/api/Talks/{id}/submissions", {
      params: { path: { id } },
      body: { conferenceId, talkTypeId },
    });

    if (error) {
      toast({ title: "Einreichung fehlgeschlagen.", variant: "destructive" });
      setSubmitting(false);
      return;
    }

    toast({ title: "Talk eingereicht — die Konferenz prüft die Einreichung." });
    navigate(`/my-talks/${id}`);
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Meine Talks", to: "/my-talks" },
          { label: "Einreichen" },
        ]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Talk einreichen</h1>

      {loadingConferences ? (
        <div className="max-w-lg space-y-4">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="max-w-lg space-y-4">
          <div>
            <label htmlFor="conference" className="mb-1.5 block text-sm font-medium">
              Konferenz
            </label>
            <select
              id="conference"
              value={conferenceId}
              onChange={(e) => setConferenceId(e.target.value)}
              required
              className="border-input bg-background focus-visible:ring-ring h-9 w-full rounded-md border px-3 text-sm focus-visible:ring-2 focus-visible:outline-none"
            >
              <option value="">Bitte wählen…</option>
              {conferences.map((conference) => (
                <option key={conference.id} value={conference.id}>
                  {conference.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label htmlFor="talkType" className="mb-1.5 block text-sm font-medium">
              Talk-Typ
            </label>
            <select
              id="talkType"
              value={talkTypeId}
              onChange={(e) => setTalkTypeId(e.target.value)}
              required
              disabled={!conferenceId || loadingTalkTypes}
              className="border-input bg-background focus-visible:ring-ring h-9 w-full rounded-md border px-3 text-sm focus-visible:ring-2 focus-visible:outline-none disabled:opacity-50"
            >
              <option value="">
                {conferenceId ? "Bitte wählen…" : "Zuerst Konferenz wählen"}
              </option>
              {talkTypes.map((talkType) => (
                <option key={talkType.id} value={talkType.id}>
                  {talkType.name} ({talkType.durationInMinutes} Min.)
                </option>
              ))}
            </select>
          </div>

          <button
            type="submit"
            disabled={submitting}
            className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-4 text-sm font-medium disabled:opacity-50"
          >
            {submitting ? "Wird eingereicht…" : "Einreichen"}
          </button>
        </form>
      )}
    </PageLayout>
  );
}
