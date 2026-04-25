import { useState, useEffect, type FormEvent } from "react";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type Profile = components["schemas"]["GetMyProfileDto"];

function SkeletonForm() {
  return (
    <div className="max-w-lg space-y-4">
      <div className="space-y-1.5">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-10 w-full" />
      </div>
      <div className="space-y-1.5">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-10 w-full" />
      </div>
      <div className="space-y-1.5">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-24 w-full" />
      </div>
    </div>
  );
}

interface FormState {
  firstName: string;
  lastName: string;
  biography: string;
}

export default function SpeakerProfilePage() {
  const { toast } = useToast();
  const [profile, setProfile] = useState<Profile | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<FormState>({
    firstName: "",
    lastName: "",
    biography: "",
  });

  useEffect(() => {
    apiClient.GET("/api/Speakers/profile").then(({ data }) => {
      if (data) {
        setProfile(data);
        setForm({
          firstName: data.firstName,
          lastName: data.lastName,
          biography: data.biography,
        });
      }
      setLoading(false);
    });
  }, []);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    try {
      if (profile) {
        const { error } = await apiClient.PUT("/api/Speakers/profile", {
          body: form,
        });
        if (error) throw new Error();
        setProfile({ ...profile, ...form });
        toast({ title: "Profil gespeichert." });
      } else {
        const { error } = await apiClient.POST("/api/Speakers/profile", {
          body: form,
        });
        if (error) throw new Error();
        // Reload to get the generated id
        const { data: reloaded } = await apiClient.GET("/api/Speakers/profile");
        if (reloaded) setProfile(reloaded);
        toast({ title: "Profil angelegt." });
      }
    } catch {
      toast({ title: "Fehler beim Speichern.", variant: "destructive" });
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <PageLayout>
        <SkeletonForm />
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      <h1 className="mb-6 text-2xl font-semibold">
        {profile ? "Profil bearbeiten" : "Profil anlegen"}
      </h1>
      <form onSubmit={handleSubmit} className="max-w-lg space-y-4">
        <div className="space-y-1.5">
          <label htmlFor="firstName" className="text-sm font-medium">
            Vorname
          </label>
          <input
            id="firstName"
            required
            value={form.firstName}
            onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))}
            className="border-input bg-background ring-offset-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
          />
        </div>

        <div className="space-y-1.5">
          <label htmlFor="lastName" className="text-sm font-medium">
            Nachname
          </label>
          <input
            id="lastName"
            required
            value={form.lastName}
            onChange={(e) => setForm((f) => ({ ...f, lastName: e.target.value }))}
            className="border-input bg-background ring-offset-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
          />
        </div>

        <div className="space-y-1.5">
          <label htmlFor="biography" className="text-sm font-medium">
            Biografie
          </label>
          <textarea
            id="biography"
            required
            rows={4}
            value={form.biography}
            onChange={(e) => setForm((f) => ({ ...f, biography: e.target.value }))}
            className="border-input bg-background ring-offset-background focus-visible:ring-ring flex w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
          />
        </div>

        <button
          type="submit"
          disabled={saving}
          className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-10 items-center rounded-md px-4 text-sm font-medium disabled:pointer-events-none disabled:opacity-50"
        >
          {saving ? "Speichern…" : "Speichern"}
        </button>
      </form>
    </PageLayout>
  );
}
