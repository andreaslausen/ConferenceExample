import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../shared/api/client";
import { PageLayout, Breadcrumbs } from "../shared/components/Layout";
import { useToast } from "../shared/components/Toast";

const INPUT_CLASS =
  "border-input bg-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none";

export default function OrganizerNewConferencePage() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [submitting, setSubmitting] = useState(false);

  const [name, setName] = useState("");
  const [start, setStart] = useState("");
  const [end, setEnd] = useState("");
  const [locationName, setLocationName] = useState("");
  const [street, setStreet] = useState("");
  const [city, setCity] = useState("");
  const [state, setState] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [country, setCountry] = useState("");

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    const { data, error } = await apiClient.POST("/api/Conferences", {
      body: {
        name,
        start: start ? new Date(start).toISOString() : undefined,
        end: end ? new Date(end).toISOString() : undefined,
        locationName,
        street,
        city,
        state,
        postalCode,
        country,
      },
    });
    if (error || !data) {
      toast({ title: "Konferenz konnte nicht erstellt werden.", variant: "destructive" });
      setSubmitting(false);
      return;
    }
    toast({ title: "Konferenz erstellt." });
    navigate(`/organizer/conferences/${data.id}`);
  }

  return (
    <PageLayout>
      <Breadcrumbs
        items={[
          { label: "Konferenzen", to: "/organizer/conferences" },
          { label: "Neue Konferenz" },
        ]}
      />
      <h1 className="mb-6 text-2xl font-semibold">Konferenz erstellen</h1>

      <form onSubmit={handleSubmit} className="max-w-lg space-y-4">
        <div className="space-y-1.5">
          <label htmlFor="name" className="text-sm font-medium">
            Name <span aria-hidden="true" className="text-destructive">*</span>
          </label>
          <input
            id="name"
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
            className={INPUT_CLASS}
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1.5">
            <label htmlFor="start" className="text-sm font-medium">
              Beginn
            </label>
            <input
              id="start"
              type="date"
              value={start}
              onChange={(e) => setStart(e.target.value)}
              className={INPUT_CLASS}
            />
          </div>
          <div className="space-y-1.5">
            <label htmlFor="end" className="text-sm font-medium">
              Ende
            </label>
            <input
              id="end"
              type="date"
              value={end}
              onChange={(e) => setEnd(e.target.value)}
              className={INPUT_CLASS}
            />
          </div>
        </div>

        <div className="space-y-1.5">
          <label htmlFor="locationName" className="text-sm font-medium">
            Veranstaltungsort <span aria-hidden="true" className="text-destructive">*</span>
          </label>
          <input
            id="locationName"
            required
            value={locationName}
            onChange={(e) => setLocationName(e.target.value)}
            className={INPUT_CLASS}
          />
        </div>

        <div className="space-y-1.5">
          <label htmlFor="street" className="text-sm font-medium">
            Straße <span aria-hidden="true" className="text-destructive">*</span>
          </label>
          <input
            id="street"
            required
            value={street}
            onChange={(e) => setStreet(e.target.value)}
            className={INPUT_CLASS}
          />
        </div>

        <div className="grid grid-cols-3 gap-3">
          <div className="space-y-1.5">
            <label htmlFor="postalCode" className="text-sm font-medium">
              PLZ <span aria-hidden="true" className="text-destructive">*</span>
            </label>
            <input
              id="postalCode"
              required
              value={postalCode}
              onChange={(e) => setPostalCode(e.target.value)}
              className={INPUT_CLASS}
            />
          </div>
          <div className="col-span-2 space-y-1.5">
            <label htmlFor="city" className="text-sm font-medium">
              Stadt <span aria-hidden="true" className="text-destructive">*</span>
            </label>
            <input
              id="city"
              required
              value={city}
              onChange={(e) => setCity(e.target.value)}
              className={INPUT_CLASS}
            />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1.5">
            <label htmlFor="state" className="text-sm font-medium">
              Bundesland / Region <span aria-hidden="true" className="text-destructive">*</span>
            </label>
            <input
              id="state"
              required
              value={state}
              onChange={(e) => setState(e.target.value)}
              className={INPUT_CLASS}
            />
          </div>
          <div className="space-y-1.5">
            <label htmlFor="country" className="text-sm font-medium">
              Land <span aria-hidden="true" className="text-destructive">*</span>
            </label>
            <input
              id="country"
              required
              value={country}
              onChange={(e) => setCountry(e.target.value)}
              className={INPUT_CLASS}
            />
          </div>
        </div>

        <div className="flex gap-2 pt-2">
          <button
            type="submit"
            disabled={submitting}
            className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex h-10 items-center rounded-md px-4 text-sm font-medium disabled:pointer-events-none disabled:opacity-50"
          >
            {submitting ? "Erstellen…" : "Erstellen"}
          </button>
          <button
            type="button"
            onClick={() => navigate("/organizer/conferences")}
            className="border-input hover:bg-accent inline-flex h-10 items-center rounded-md border px-4 text-sm"
          >
            Abbrechen
          </button>
        </div>
      </form>
    </PageLayout>
  );
}
