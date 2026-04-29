import { useState, useEffect, type FormEvent } from "react";
import { useParams, useNavigate } from "react-router-dom";
import apiClient from "../shared/api/client";
import type { components } from "../shared/api/openapi.d";
import { Skeleton } from "../shared/components/Skeleton";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

type ConferenceDetail = components["schemas"]["GetConferenceByIdDto"];

function toLocalDateTimeValue(isoString: string | null | undefined): string {
  if (!isoString) return "";
  // Convert ISO string to local datetime-local format (YYYY-MM-DDTHH:MM)
  const date = new Date(isoString);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

const STATUS_ORDER = {
  Draft: 0,
  CallForSpeakers: 1,
  CallForSpeakersClosed: 2,
  ProgramPublished: 3,
} as const;

export default function EditConferencePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();

  const [conference, setConference] = useState<ConferenceDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  // Form state
  const [name, setName] = useState("");
  const [start, setStart] = useState("");
  const [end, setEnd] = useState("");
  const [locationName, setLocationName] = useState("");
  const [street, setStreet] = useState("");
  const [city, setCity] = useState("");
  const [state, setState] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [country, setCountry] = useState("");

  // Check if conference is editable (only Draft status can be edited)
  const isEditable =
    conference?.status &&
    STATUS_ORDER[conference.status as keyof typeof STATUS_ORDER] <
    STATUS_ORDER.CallForSpeakers;

  useEffect(() => {
    if (!id) return;
    apiClient
      .GET("/api/Conferences/{id}", { params: { path: { id } } })
      .then(({ data }) => {
        if (!data) return;
        setConference(data);
        setName(data.name);
        setStart(toLocalDateTimeValue(data.startDate));
        setEnd(toLocalDateTimeValue(data.endDate));
        setLocationName(data.locationName);
        setStreet(data.street);
        setCity(data.city);
        setState(data.state);
        setPostalCode(data.postalCode);
        setCountry(data.country);
        setLoading(false);
      });
  }, [id]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!id) return;

    setSaving(true);
    const { error } = await apiClient.PUT("/api/Conferences/{id}/details", {
      params: { path: { id } },
      body: {
        name: name.trim(),
        start: start || null,
        end: end || null,
        locationName: locationName.trim(),
        street: street.trim(),
        city: city.trim(),
        state: state.trim(),
        postalCode: postalCode.trim(),
        country: country.trim(),
      },
    });
    setSaving(false);

    if (error) {
      toast({ title: "Speichern fehlgeschlagen.", variant: "destructive" });
      return;
    }

    toast({ title: "Konferenzdetails gespeichert." });
    navigate(`/organizer/conferences/${id}`);
  }

  if (loading) {
    return (
      <PageLayout>
        <Skeleton className="mb-4 h-8 w-64" />
        <Skeleton className="h-96 w-full" />
      </PageLayout>
    );
  }

  if (!conference) {
    return (
      <PageLayout>
        <p className="text-destructive text-sm">Konferenz nicht gefunden.</p>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Konferenzen", to: "/organizer/conferences" },
          { label: conference.name, to: `/organizer/conferences/${id}` },
          { label: "Bearbeiten" },
        ]}
      />

      <h1 className="mb-6 text-2xl font-semibold">Konferenz bearbeiten</h1>

      {!isEditable && (
        <div className="mb-6 rounded-md border border-amber-500 bg-amber-50 p-4 dark:bg-amber-950/30">
          <p className="text-sm text-amber-800 dark:text-amber-200">
            Diese Konferenz kann nicht mehr bearbeitet werden, da sie bereits
            im Status "{conference.status}" ist. Nur Konferenzen im Status
            "Draft" können bearbeitet werden.
          </p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="max-w-2xl space-y-6">
        {/* Name */}
        <div>
          <label htmlFor="name" className="mb-1 block text-sm font-medium">
            Name <span className="text-destructive">*</span>
          </label>
          <input
            id="name"
            type="text"
            required
            disabled={!isEditable}
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
          />
        </div>

        {/* Dates */}
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="start" className="mb-1 block text-sm font-medium">
              Startdatum
            </label>
            <input
              id="start"
              type="datetime-local"
              disabled={!isEditable}
              value={start}
              onChange={(e) => setStart(e.target.value)}
              className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
          <div>
            <label htmlFor="end" className="mb-1 block text-sm font-medium">
              Enddatum
            </label>
            <input
              id="end"
              type="datetime-local"
              disabled={!isEditable}
              value={end}
              onChange={(e) => setEnd(e.target.value)}
              className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
        </div>

        {/* Location */}
        <div>
          <label htmlFor="locationName" className="mb-1 block text-sm font-medium">
            Veranstaltungsort <span className="text-destructive">*</span>
          </label>
          <input
            id="locationName"
            type="text"
            required
            disabled={!isEditable}
            value={locationName}
            onChange={(e) => setLocationName(e.target.value)}
            className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
          />
        </div>

        <div>
          <label htmlFor="street" className="mb-1 block text-sm font-medium">
            Straße <span className="text-destructive">*</span>
          </label>
          <input
            id="street"
            type="text"
            required
            disabled={!isEditable}
            value={street}
            onChange={(e) => setStreet(e.target.value)}
            className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="city" className="mb-1 block text-sm font-medium">
              Stadt <span className="text-destructive">*</span>
            </label>
            <input
              id="city"
              type="text"
              required
              disabled={!isEditable}
              value={city}
              onChange={(e) => setCity(e.target.value)}
              className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
          <div>
            <label htmlFor="state" className="mb-1 block text-sm font-medium">
              Bundesland <span className="text-destructive">*</span>
            </label>
            <input
              id="state"
              type="text"
              required
              disabled={!isEditable}
              value={state}
              onChange={(e) => setState(e.target.value)}
              className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="postalCode" className="mb-1 block text-sm font-medium">
              PLZ <span className="text-destructive">*</span>
            </label>
            <input
              id="postalCode"
              type="text"
              required
              disabled={!isEditable}
              value={postalCode}
              onChange={(e) => setPostalCode(e.target.value)}
              className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
          <div>
            <label htmlFor="country" className="mb-1 block text-sm font-medium">
              Land <span className="text-destructive">*</span>
            </label>
            <input
              id="country"
              type="text"
              required
              disabled={!isEditable}
              value={country}
              onChange={(e) => setCountry(e.target.value)}
              className="border-input bg-background focus-visible:ring-ring w-full rounded-md border px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
        </div>

        {/* Submit buttons */}
        <div className="flex gap-3">
          <button
            type="submit"
            disabled={saving || !isEditable}
            className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-10 items-center rounded-md px-4 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
          >
            {saving ? "Speichern..." : "Speichern"}
          </button>
          <button
            type="button"
            onClick={() => navigate(`/organizer/conferences/${id}`)}
            className="border-input hover:bg-accent inline-flex h-10 items-center rounded-md border px-4 text-sm font-medium"
          >
            Abbrechen
          </button>
        </div>
      </form>
    </PageLayout>
  );
}
