# Frontend-Implementierungsplan

## Übersicht

Technologie-Stack: React + Vite + TypeScript, Tailwind CSS, shadcn/ui, @dnd-kit  
Zielgruppen: Attendees (öffentlich), Speakers (eingeloggt, Rolle `Speaker`), Organizers (eingeloggt, Rolle `Organizer`)

---

## Screens & User Flows

### Öffentlich (Attendee)

```
/                       → Konferenzliste
/conferences/:id        → Konferenzprogramm (Zeitplan + Räume)
/talks/:id              → Talk-Detail (Titel, Abstract, Speaker, Tags)
```

### Speaker

```
/register               → Registrierung (Rolle: Speaker)
/login                  → Login
/profile                → Speaker-Profil anlegen / bearbeiten
/my-talks               → Meine eingereichten Talks
/my-talks/submit        → Neuen Talk einreichen
/my-talks/:id/edit      → Talk bearbeiten
```

### Organizer

```
/register               → Registrierung (Rolle: Organizer)
/login                  → Login (gleicher Screen)
/organizer/conferences                          → Konferenzverwaltung (Liste)
/organizer/conferences/new                      → Konferenz erstellen
/organizer/conferences/:id                      → Konferenzdetail (Organizer-Ansicht)
/organizer/conferences/:id/talk-types           → Talk-Typen verwalten
/organizer/conferences/:id/rooms                → Räume verwalten
/organizer/conferences/:id/proposals            → Einreichungen prüfen (Accept/Reject)
/organizer/conferences/:id/schedule             → Zeitplan-Editor (Drag & Drop)
```

---

## Meilensteine

---

### Meilenstein 1 — Backend-Erweiterungen als Frontend-Voraussetzung

Ziel: Das Backend liefert alle Daten, die das Frontend benötigt. Ohne diese Änderungen ist ein vollständiges Frontend nicht möglich.

#### 1a — CORS & Infrastruktur

- [x] **M1-1** CORS-Middleware in `Program.cs` konfigurieren: in Development alle Origins erlauben, in Production die Frontend-URL als erlaubten Origin eintragen
  > Begründung: Das Frontend läuft als eigenständige SPA auf einem anderen Origin als das Backend. Ohne CORS werden alle API-Requests vom Browser blockiert. Vite-Proxy löst das nur für den lokalen Dev-Server, nicht für Production-Deployments.

#### 1b — Öffentlicher Programm-Endpunkt

- [x] **M1-2** Neuen Endpunkt `GET /api/conferences/:id/program` anlegen mit `[AllowAnonymous]`
  > Begründung: Der bestehende `GET /api/conferences/:id/schedule` ist auf `[Authorize(Roles = "Organizer")]` eingeschränkt. Attendees können das Konferenzprogramm ohne Login nicht sehen. Ein separater Endpunkt ist sauberer als `[AllowAnonymous]` auf dem Organizer-Endpunkt, da beide ggf. unterschiedliche Projektionen brauchen.
- [x] **M1-3** `GetConferenceProgramDto` anlegen mit: `TalkId`, `TalkTitle`, `SpeakerName`, `SlotStart`, `SlotEnd`, `RoomId`, `RoomName`
  > Begründung: `GetConferenceScheduleDto` hat kein `TalkTitle` und keinen `SpeakerName`. Ohne Titel kann der Zeitplan-Grid nicht befüllt werden; ohne Speaker-Namen fehlt eine wichtige Info auf der öffentlichen Programmseite.

#### 1c — DTOs mit fehlenden Feldern ergänzen

- [x] **M1-4** `GetConferenceScheduleDto` (Organizer-Schedule) um `TalkTitle` ergänzen
  > Begründung: Der Zeitplan-Editor zeigt Talks in den Grid-Zellen an — ohne Titel ist die Zelle leer.
- [x] **M1-5** `GetTalkByIdDto` um `SpeakerId` und `SpeakerName` ergänzen
  > Begründung: Die öffentliche Talk-Detailseite soll den Vortragenden anzeigen. Aktuell fehlen beide Felder im DTO, sodass das Frontend keinen Speaker-Bezug herstellen kann.
- [x] **M1-6** `GetConferenceTalksDto` (Organizer-Proposals) um `SpeakerName` ergänzen
  > Begründung: Die Proposals-Liste zeigt Talks inkl. Einreicher. Aktuell ist nur `SpeakerId` vorhanden, was N separate `GET /api/speakers/:id`-Aufrufe pro Zeile erzwingen würde.

#### 1d — Raum-Verwaltung

- [x] **M1-7** Endpunkt `POST /api/conferences/:id/rooms` anlegen (Body: `{ name: string }`) mit `[Authorize(Roles = "Organizer")]`
  > Begründung: Räume werden aktuell implizit über `AssignTalkToRoom` erstellt (Name + neue GUID). Der Zeitplan-Editor braucht aber vorab definierte Räume als feste Spalten im Grid. Ohne explizite Raumverwaltung gibt es nichts anzuzeigen, bevor der erste Talk eingeplant wird.
- [x] **M1-8** Endpunkt `GET /api/conferences/:id/rooms` anlegen mit `[Authorize(Roles = "Organizer")]`
  > Begründung: Der Zeitplan-Editor (M7) rendert Räume als Spalten. Diese Spalten müssen aus dem Backend geladen werden.
- [x] **M1-9** Endpunkt `DELETE /api/conferences/:id/rooms/:roomId` anlegen mit `[Authorize(Roles = "Organizer")]`
  > Begründung: Räume sollen auch wieder entfernt werden können (analog zu Talk-Types). Vollständige CRUD-Symmetrie verhindert Daten-Leichen im System.
- [x] **M1-10** `AssignTalkToRoomDto` so anpassen, dass nur noch `RoomId` übergeben wird (kein `RoomName` mehr)
  > Begründung: Sobald Räume explizit verwaltet werden, ist das Mitschicken des Namens beim Zuweisen redundant und fehleranfällig (Name könnte vom gespeicherten Raumnamen abweichen). Der Name wird stattdessen aus dem Raum-Endpunkt gelesen.

#### 1e — OpenAPI-Spec aktualisieren

- [x] **M1-11** `./scripts/generate-openapi.sh` ausführen und `openapi.json` committen, nachdem alle obigen Änderungen implementiert sind
  > Begründung: Der TypeScript-API-Client im Frontend wird aus der OpenAPI-Spec generiert. Eine veraltete Spec würde veraltete Typen erzeugen.

---

### Meilenstein 2 — Frontend-Projektsetup & Infrastruktur

Ziel: Lauffähiges Grundgerüst, das alle späteren Features trägt.

#### Aufgaben

- [x] **M2-1** Vite-Projekt mit React und TypeScript anlegen (`npm create vite@latest`)
- [x] **M2-2** Tailwind CSS v4 installieren und konfigurieren
- [x] **M2-3** shadcn/ui initialisieren (`npx shadcn@latest init`)
- [x] **M2-4** Verzeichnisstruktur anlegen:
  ```
  src/
    features/
      conference/   ← Conference Bounded Context
      talk/         ← Talk Bounded Context
    shared/
      api/          ← API-Client
      auth/         ← Auth-Context + Hooks
      components/   ← gemeinsame UI-Komponenten
      lib/          ← Hilfsfunktionen
  ```
- [x] **M2-5** React Router v7 installieren und Root-Router in `main.tsx` konfigurieren
- [x] **M2-6** TypeScript-Client aus `openapi.json` mit `openapi-typescript` oder `orval` generieren
- [x] **M2-7** Vite-Dev-Proxy konfigurieren: `/api` → `https://localhost:5001` (löst CORS im Dev-Betrieb ohne Browser-Einschränkungen)
- [x] **M2-8** Axios-Instanz mit Base-URL und Bearer-Token-Interceptor anlegen
- [x] **M2-9** Platzhalter-Routen für alle Screens anlegen (leere Komponenten), damit der Router vollständig verdrahtet ist
- [x] **M2-10** Globale Fehlerbehandlung: `ErrorBoundary`-Komponente und 404-Seite

---

### Meilenstein 3 — Authentifizierung

Ziel: Benutzer können sich registrieren und einloggen; Authentifizierungsstatus ist app-weit verfügbar.

#### Aufgaben

- [x] **M3-1** `AuthContext` mit `user`, `token`, `login()`, `logout()`, `register()` implementieren
- [x] **M3-2** JWT-Token im `localStorage` persistieren und beim App-Start wiederherstellen
- [x] **M3-3** Axios-Interceptor den Token aus dem Context lesen lassen
- [x] **M3-4** `ProtectedRoute`-Wrapper: leitet nicht authentifizierte Nutzer auf `/login` um
- [x] **M3-5** `RoleGuard`-Wrapper: leitet Nutzer ohne passende Rolle auf 403-Seite um
- [x] **M3-6** Login-Screen mit E-Mail/Passwort-Formular und shadcn/ui `Form`-Komponenten
- [x] **M3-7** Registrierungs-Screen mit Rollenwahl (`Speaker` / `Organizer`) als Radio-Group
- [x] **M3-8** Fehlerbehandlung für ungültige Credentials (API-Fehler in Formularen anzeigen)
- [x] **M3-9** Globaler Header mit Login/Logout-Button und rollenbasierter Navigation

---

### Meilenstein 4 — Öffentliche Konferenzübersicht (Attendee)

Ziel: Nicht eingeloggte Nutzer können Konferenzen und das Programm einsehen.

#### Aufgaben

- [ ] **M4-1** `GET /api/conferences` aufrufen; Konferenzliste als Karten-Grid rendern
- [ ] **M4-2** Pagination für die Konferenzliste implementieren (shadcn/ui `Pagination`)
- [ ] **M4-3** `GET /api/conferences/:id` aufrufen; Konferenzdetail-Header rendern (Name, Status)
- [ ] **M4-4** `GET /api/conferences/:id/program` aufrufen; Zeitplan als Tabelle (Zeitslots × Räume) rendern
- [ ] **M4-5** Klick auf Talk in der Zeitplan-Tabelle navigiert zu `/talks/:id`
- [ ] **M4-6** `GET /api/talks/:id` aufrufen; Talk-Detailseite rendern (Titel, Abstract, Tags, Speaker-Name)
- [ ] **M4-7** Loading-Skeletons (shadcn/ui `Skeleton`) für alle Datenladeoperationen
- [ ] **M4-8** Leerzustand-Komponente für Konferenzen ohne Einträge im Zeitplan

---

### Meilenstein 5 — Speaker-Bereich

Ziel: Speaker können ihr Profil verwalten und Talks einreichen/bearbeiten.

#### Aufgaben

**Speaker-Profil**

- [ ] **M5-1** `GET /api/speakers/profile` aufrufen; Profildaten anzeigen
- [ ] **M5-2** Profil-Formular (Name, Bio) für `POST /api/speakers/profile` implementieren
- [ ] **M5-3** Profil-Formular für `PUT /api/speakers/profile` implementieren (gleiche Komponente, anderer Submit-Handler)
- [ ] **M5-4** Weiterleitung nach dem ersten Anlegen auf `/profile` statt auf ein leeres Formular

**Talk-Einreichung**

- [ ] **M5-5** `GET /api/conferences` aufrufen und Konferenzauswahl als Dropdown in Submit-Formular einbinden
- [ ] **M5-6** `GET /api/conferences/:id/talk-types` aufrufen und Talk-Typ-Auswahl als Dropdown einbinden
- [ ] **M5-7** Submit-Formular mit Feldern: Titel, Abstract, Tags (Chip-Input), Konferenz, Talk-Typ
- [ ] **M5-8** `POST /api/talks` aufrufen und nach Erfolg auf `/my-talks` weiterleiten
- [ ] **M5-9** `GET /api/talks/my-talks` aufrufen; eigene Talks als Liste rendern (Titel, Status-Badge)
- [ ] **M5-10** Edit-Formular mit `GET /api/talks/:id` vorausfüllen
- [ ] **M5-11** `PUT /api/talks/:id` aufrufen und nach Erfolg auf `/my-talks` weiterleiten
- [ ] **M5-12** Inline-Validierung: Titel und Abstract sind Pflichtfelder

---

### Meilenstein 6 — Organizer-Bereich: Konferenz-, Raum- & Talk-Type-Verwaltung

Ziel: Organizer können Konferenzen anlegen, umbenennen, Räume und Talk-Typen definieren.

#### Aufgaben

**Konferenz-Verwaltung**

- [ ] **M6-1** Konferenzliste für Organizer aus `GET /api/conferences` mit eigenem Layout (Tabelle statt Karten)
- [ ] **M6-2** Formular für `POST /api/conferences` (Name, Datum); nach Erfolg auf Detailseite weiterleiten
- [ ] **M6-3** Inline-Umbenennung: Klick auf Konferenzname öffnet Eingabefeld → `PUT /api/conferences/:id/name`
- [ ] **M6-4** Status-Änderung: Dropdown/Button-Gruppe → `PUT /api/conferences/:id/status` (z. B. Draft → Published)

**Raum-Verwaltung**

- [ ] **M6-5** Raum-Liste aus `GET /api/conferences/:id/rooms` anzeigen
- [ ] **M6-6** Formular für `POST /api/conferences/:id/rooms` (Name)
- [ ] **M6-7** Löschen-Button pro Raum → `DELETE /api/conferences/:id/rooms/:roomId` mit Bestätigungsdialog

**Talk-Typen**

- [ ] **M6-8** Talk-Typen-Liste aus `GET /api/conferences/:id/talk-types` anzeigen
- [ ] **M6-9** Formular für `POST /api/conferences/:id/talk-types` (Name, Dauer in Minuten)
- [ ] **M6-10** Löschen-Button pro Talk-Typ → `DELETE /api/conferences/:id/talk-types/:talkTypeId` mit Bestätigungsdialog

---

### Meilenstein 7 — Organizer-Bereich: Einreichungen prüfen

Ziel: Organizer können eingereichte Talks akzeptieren oder ablehnen.

#### Aufgaben

- [ ] **M7-1** `GET /api/conferences/:id/talks` aufrufen; Einreichungen als Liste rendern (inkl. `SpeakerName` aus erweitertem DTO)
- [ ] **M7-2** Statusfilter (Alle / Pending / Accepted / Rejected) als Tab-Leiste
- [ ] **M7-3** Pagination für die Einreichungsliste
- [ ] **M7-4** Accept-Button → `PUT /api/conferences/:id/talks/:talkId/accept`; optimistisches UI-Update
- [ ] **M7-5** Reject-Button → `PUT /api/conferences/:id/talks/:talkId/reject`; optimistisches UI-Update
- [ ] **M7-6** Klick auf einen Talk öffnet Detail-Drawer (shadcn/ui `Sheet`) mit Titel, Abstract, Speaker

---

### Meilenstein 8 — Organizer-Bereich: Zeitplan-Editor (Drag & Drop)

Ziel: Organizer können akzeptierte Talks per Drag & Drop in Zeitslots und Räume einplanen.

#### Aufgaben

- [ ] **M8-1** `@dnd-kit/core` und `@dnd-kit/sortable` installieren
- [ ] **M8-2** Räume aus `GET /api/conferences/:id/rooms` laden und als Spalten im Grid rendern
- [ ] **M8-3** Zeitplan-Grid-Komponente: Achsen Zeitslots (Zeilen) × Räume (Spalten)
- [ ] **M8-4** Seitenleiste mit nicht eingeplanten (accepted) Talks als Drag-Quellen
- [ ] **M8-5** Zellen im Grid als Drop-Targets implementieren
- [ ] **M8-6** Beim Drop: `PUT /api/conferences/:id/talks/:talkId/schedule` mit Zeitslot aufrufen
- [ ] **M8-7** Beim Drop auf andere Spalte: `PUT /api/conferences/:id/talks/:talkId/room` mit `RoomId` aufrufen (kein `RoomName` mehr nötig, da M1-10)
- [ ] **M8-8** Visuelles Feedback während Drag (Ghost-Element, Hover-Highlight auf Zielzelle)
- [ ] **M8-9** Fehlerbehandlung: API-Fehler rückgängig machen und Toast-Nachricht anzeigen (shadcn/ui `Toast`)
- [ ] **M8-10** Bereits eingeplante Talks können per Drag innerhalb des Grids verschoben werden

---

### Meilenstein 9 — Polish & Qualitätssicherung

Ziel: Konsistente UX, Barrierefreiheit und stabile Basis.

#### Aufgaben

- [ ] **M9-1** Rollenbasierte Navigation im Header vervollständigen (Links je nach Rolle ein-/ausblenden)
- [ ] **M9-2** Breadcrumb-Navigation für tief verschachtelte Organizer-Seiten
- [ ] **M9-3** Globale Toast/Notification-Infrastruktur für Erfolgsmeldungen
- [ ] **M9-4** Alle Formulare: Keyboard-Navigation und ARIA-Labels prüfen
- [ ] **M9-5** Responsive Layout: Zeitplan-Grid auf Mobilgeräten als vertikale Liste darstellen
- [ ] **M9-6** API-Fehler-Kategorisierung: 401 → Login-Redirect, 403 → Zugriffsfehler-Banner, 5xx → generische Fehlermeldung
- [ ] **M9-7** TypeScript `strict`-Mode aktivieren; alle `any`-Typen entfernen
- [ ] **M9-8** OpenAPI-generierten Client in CI einbinden (Build schlägt fehl, wenn `openapi.json` veraltet ist)

---

## Abhängigkeiten zwischen Meilensteinen

```
M1 (Backend-Erweiterungen)
  └─► M2 (Frontend-Setup)
        └─► M3 (Auth)
              ├─► M4 (Attendee)
              ├─► M5 (Speaker)      ← benötigt M4 für Konferenz-/Talk-Type-Auswahl
              ├─► M6 (Organizer Basis)
              │     └─► M7 (Proposals)
              │           └─► M8 (Drag & Drop Schedule)
              └─► M9 (Polish)       ← parallel zu M4–M8, abschließend
```

---

## Entschiedene Fragen

| # | Frage | Entscheidung |
|---|-------|--------------|
| 1 | Welcher Identity Provider? | Aktueller JWT-Auth-Controller (E-Mail + Passwort) genügt zunächst |
| 2 | Vite-Proxy oder CORS? | Beides: Vite-Proxy für Dev (M2-7), CORS-Middleware im Backend für Production (M1-1) |
| 3 | Slide Sharing? | Nicht in diesem Plan — kommt später |
| 4 | Real-time-Benachrichtigungen? | Nicht in diesem Plan — kommt später |
