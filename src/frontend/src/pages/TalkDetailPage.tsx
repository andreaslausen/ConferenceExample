import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";

type Talk = components["schemas"]["GetTalkByIdDto"];

function TalkDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-8 w-3/4" />
      <div className="space-y-2">
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-2/3" />
      </div>
      <div className="flex gap-2">
        <Skeleton className="h-6 w-16 rounded-full" />
        <Skeleton className="h-6 w-20 rounded-full" />
        <Skeleton className="h-6 w-14 rounded-full" />
      </div>
    </div>
  );
}

export default function TalkDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [talk, setTalk] = useState<Talk | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    setLoading(true);
    setError(null);

    apiClient
      .GET("/api/Talks/{id}", { params: { path: { id } } })
      .then(({ data, error: apiError }) => {
        if (cancelled) return;
        if (apiError || !data) {
          setError("Talk konnte nicht geladen werden.");
        } else {
          setTalk(data);
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      {loading ? (
        <TalkDetailSkeleton />
      ) : error || !talk ? (
        <p role="alert" className="text-destructive text-sm">
          {error ?? "Talk nicht gefunden."}
        </p>
      ) : (
        <>
          <div className="mb-6">
            <Link
              to={`/conferences/${talk.conferenceId}`}
              className="text-muted-foreground hover:text-foreground mb-4 inline-flex items-center gap-1 text-sm"
            >
              ← Zurück zum Programm
            </Link>
          </div>

          <h1 className="mb-2 text-2xl font-semibold">{talk.title}</h1>

          <p className="text-muted-foreground mb-6 text-sm">
            {talk.speakerName}
          </p>

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

          <div className="prose prose-sm text-foreground max-w-none">
            <p className="leading-relaxed whitespace-pre-wrap">{talk.abstract}</p>
          </div>
        </>
      )}
    </div>
  );
}
