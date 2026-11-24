import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";

type Talk = components["schemas"]["GetTalkByIdDto"];
type Submission = components["schemas"]["GetTalkSubmissionsDto"];

const POLL_INTERVAL_MS = 3000;

const STATUS_LABELS: Record<string, string> = {
  Pending: "Wird geprüft",
  Submitted: "Eingereicht",
  Accepted: "Angenommen",
  Rejected: "Abgelehnt",
  Failed: "Nicht möglich",
};

const STATUS_COLORS: Record<string, string> = {
  Pending: "bg-muted text-muted-foreground",
  Submitted: "bg-secondary text-secondary-foreground",
  Accepted: "bg-primary/10 text-primary",
  Rejected: "bg-destructive/10 text-destructive",
  Failed: "bg-destructive/10 text-destructive",
};

function StatusBadge({ status }: { status: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${STATUS_COLORS[status] ?? "bg-muted text-muted-foreground"}`}
    >
      {STATUS_LABELS[status] ?? status}
    </span>
  );
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString("de-DE", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

/**
 * A speaker's own talk, with every conference it was submitted to and what came of it. The
 * submitted content is a snapshot — it does not change when the talk is edited afterwards.
 */
export default function TalkDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [talk, setTalk] = useState<Talk | null>(null);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    let pollTimeout: ReturnType<typeof setTimeout> | undefined;
    setLoading(true);
    setError(null);

    async function load(showSkeleton: boolean) {
      if (showSkeleton) setLoading(true);

      const [talkResult, submissionsResult] = await Promise.all([
        apiClient.GET("/api/Talks/{id}", { params: { path: { id: id! } } }),
        apiClient.GET("/api/Talks/{id}/submissions", { params: { path: { id: id! } } }),
      ]);
      if (cancelled) return;

      if (talkResult.error || !talkResult.data) {
        setError("Talk konnte nicht geladen werden.");
        setLoading(false);
        return;
      }

      setTalk(talkResult.data);
      const items = submissionsResult.data ?? [];
      setSubmissions(items);
      setLoading(false);

      // A fresh submission stays Pending until the conference has processed it — poll quietly
      // so the badge settles on its own instead of the speaker having to reload.
      if (items.some((submission) => submission.status === "Pending")) {
        pollTimeout = setTimeout(() => load(false), POLL_INTERVAL_MS);
      }
    }

    load(true);

    return () => {
      cancelled = true;
      if (pollTimeout) clearTimeout(pollTimeout);
    };
  }, [id]);

  if (loading) {
    return (
      <PageLayout>
        <div className="space-y-6">
          <Skeleton className="h-8 w-3/4" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-2/3" />
        </div>
      </PageLayout>
    );
  }

  if (error || !talk) {
    return (
      <PageLayout>
        <p role="alert" className="text-destructive text-sm">
          {error ?? "Talk nicht gefunden."}
        </p>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      <Breadcrumbs items={[{ label: "Meine Talks", to: "/my-talks" }, { label: talk.title }]} />

      <div className="mb-6 flex items-start justify-between gap-4">
        <h1 className="text-2xl font-semibold">{talk.title}</h1>
        <div className="flex shrink-0 gap-2">
          <Link
            to={`/my-talks/${talk.id}/submit`}
            className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-9 items-center rounded-md px-3 text-sm font-medium"
          >
            Einreichen
          </Link>
          <Link
            to={`/my-talks/${talk.id}/edit`}
            className="border-input hover:bg-accent inline-flex h-9 items-center rounded-md border px-3 text-sm"
          >
            Bearbeiten
          </Link>
        </div>
      </div>

      {talk.tags.length > 0 && (
        <div className="mb-6 flex flex-wrap gap-2">
          {talk.tags.map((tag) => (
            <span
              key={tag}
              className="bg-secondary text-secondary-foreground rounded-full px-2.5 py-0.5 text-xs font-medium"
            >
              {tag}
            </span>
          ))}
        </div>
      )}

      <p className="text-foreground mb-10 leading-relaxed whitespace-pre-wrap">{talk.abstract}</p>

      <h2 className="mb-3 text-lg font-semibold">Einreichungen</h2>

      {submissions.length === 0 ? (
        <p className="text-muted-foreground text-sm">
          Dieser Talk ist noch nirgends eingereicht.
        </p>
      ) : (
        <div className="space-y-2">
          {submissions.map((submission) => (
            <div
              key={submission.conferenceId}
              className="border-border rounded-lg border p-4"
            >
              <div className="flex items-center justify-between gap-4">
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium">
                    {submission.conferenceName || submission.conferenceId}
                  </p>
                  <p className="text-muted-foreground mt-0.5 text-xs">
                    Eingereicht am {formatDate(submission.submittedAt)} als „{submission.title}“
                  </p>
                </div>
                <StatusBadge status={submission.status} />
              </div>
              {submission.reason && (
                <p className="text-muted-foreground mt-2 text-xs">{submission.reason}</p>
              )}
            </div>
          ))}
        </div>
      )}
    </PageLayout>
  );
}
