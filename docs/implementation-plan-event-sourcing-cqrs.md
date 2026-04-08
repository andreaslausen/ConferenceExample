# Implementierungsplan: Event Sourcing & CQRS

## Zielbild

Requests kommen am API-Controller an und werden an Command- oder QueryHandler delegiert (CQRS). Commands erzeugen Domain Events, die im zentralen EventStore persistiert werden. Aggregates werden durch Event Replay aus dem EventStore geladen. Event Handler reagieren auf Events und koennen spaeter ReadModels aktualisieren. Die Kommunikation zwischen Bounded Contexts erfolgt ueber ein Observer Pattern (EventBus).

Keine externen Libraries (kein MediatR o.ae.) -- alles Eigenimplementierung.

## Architektur nach Umbau

```
API Controller
  |
  +--> CommandHandler --> Aggregate (Domain) --> Events --> EventStore
  |                                                    --> EventBus (Observer)
  +--> QueryHandler  --> Repository (laedt via Event Replay aus EventStore)

EventBus --> EventHandler (pro BC, reagiert auf Events anderer BCs)
```

### Projektstruktur (neu)

```
ConferenceExample.EventStore          (NEU - zentrales Projekt)
  ├── IEventStore.cs
  ├── InMemoryEventStore.cs
  ├── IEventBus.cs
  ├── InMemoryEventBus.cs
  └── StoredEvent.cs

ConferenceExample.API                 (angepasst)
  ├── Controllers/                    (NEU - echte Endpoints)
  ├── Extensions/ServiceCollectionExtensions.cs  (angepasst)
  └── Program.cs                      (angepasst)

Conference BC:
  ConferenceExample.Conference.Domain        (angepasst - Events, AggregateRoot, Aggregate)
  ConferenceExample.Conference.Application   (angepasst - Command/QueryHandler)
  ConferenceExample.Conference.Persistence   (angepasst - Repository mit Event Replay)

Session BC:
  ConferenceExample.Session.Domain           (angepasst - Events, AggregateRoot, Aggregate)
  ConferenceExample.Session.Application      (angepasst - Command/QueryHandler)
  ConferenceExample.Session.Persistence      (angepasst - Repository mit Event Replay)

ConferenceExample.Persistence               (ENTFERNEN)
```

**Wichtig:** `IDomainEvent` und `AggregateRoot` werden in jedem Domain-Projekt eigenstaendig definiert (Duplikation statt Referenz). So bleiben die Domain-Projekte frei von externen Abhaengigkeiten. Nur die Persistence-Projekte referenzieren `ConferenceExample.EventStore`.

---

## Schritt 1: Zentrales EventStore-Projekt anlegen ✅

Neues Projekt `ConferenceExample.EventStore` erstellen. Dieses Projekt enthaelt die Infrastruktur fuer Event-Persistierung und den EventBus. Es kennt keine Domain-Events direkt -- es arbeitet mit serialisierten Events (`StoredEvent`).

### 1.1 Projekt anlegen und zur Solution hinzufuegen ✅

Neues Class-Library-Projekt `ConferenceExample.EventStore` erstellen und in `ConferenceExample.sln` einbinden.

### 1.2 StoredEvent und IEventStore ✅

**`StoredEvent`** -- Wrapper fuer persistierte Events:
```csharp
public record StoredEvent(
    Guid Id,
    Guid AggregateId,
    string EventType,         // Assembly-qualified Type Name
    string Payload,           // JSON-serialisiertes Event
    DateTimeOffset OccurredAt,
    long Version);
```

**`IEventStore`**:
```csharp
public interface IEventStore
{
    Task AppendEvents(Guid aggregateId, IEnumerable<StoredEvent> events, long expectedVersion);
    Task<IReadOnlyList<StoredEvent>> GetEvents(Guid aggregateId);
    Task<IReadOnlyList<StoredEvent>> GetAllEvents();  // fuer spaetere Projektionen
}
```

**`InMemoryEventStore`** -- Thread-sichere In-Memory-Implementierung mit einer Liste von `StoredEvent`. Optimistic Concurrency ueber `expectedVersion`.

### 1.3 EventBus (Observer Pattern) ✅

**`IEventBus`**:
```csharp
public interface IEventBus
{
    void Subscribe(string eventType, Action<StoredEvent> handler);
    Task Publish(IEnumerable<StoredEvent> events);
}
```

**`InMemoryEventBus`** -- Haelt eine `Dictionary<string, List<Action<StoredEvent>>>` fuer Subscriptions. `Publish` iteriert ueber alle registrierten Handler fuer den jeweiligen Event-Typ.

Der EventBus arbeitet auf `StoredEvent`-Ebene. Die Deserialisierung in konkrete Domain-Event-Typen erfolgt in den jeweiligen Bounded Contexts.

### 1.4 Build verifizieren ✅

`dotnet build` -- das neue Projekt muss eigenstaendig kompilieren.

---

## Schritt 2: Session-Domain umbauen (Session.Domain)

### 2.1 IDomainEvent und AggregateRoot anlegen

`IDomainEvent` (Marker-Interface) und `AggregateRoot` (Basisklasse) direkt im Session.Domain-Projekt anlegen -- keine externe Abhaengigkeit.

**`IDomainEvent`**:
```csharp
public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccurredAt { get; }
}
```

**`AggregateRoot`**:
```csharp
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    public long Version { get; protected set; } = -1;

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents;
    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void RaiseEvent(IDomainEvent @event)
    {
        ApplyEvent(@event);
        _uncommittedEvents.Add(@event);
    }

    public void ReplayEvents(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyEvent(@event);
            Version++;
        }
    }

    protected abstract void ApplyEvent(IDomainEvent @event);
}
```

### 2.2 Domain Events definieren

Events fuer die Session-Entitaet:
- `SessionSubmittedEvent` -- enthaelt alle initialen Daten (Title, Abstract, SpeakerId, Tags, SessionTypeId, ConferenceId)
- `SessionTitleEditedEvent` -- enthaelt neuen Title
- `SessionAbstractEditedEvent` -- enthaelt neues Abstract
- `SessionTagAddedEvent` -- enthaelt Tag
- `SessionTagRemovedEvent` -- enthaelt Tag

### 2.3 Session-Aggregate umbauen

`Session` erbt von `AggregateRoot`. Zustandsaenderungen erfolgen nur noch ueber `RaiseEvent()`:

```csharp
public class Session : AggregateRoot
{
    // Properties bleiben, aber alle Mutationen gehen ueber Events

    public static Session Submit(SessionId id, SessionTitle title, ...)
    {
        var session = new Session();
        session.RaiseEvent(new SessionSubmittedEvent(...));
        return session;
    }

    public void EditTitle(SessionTitle title)
    {
        RaiseEvent(new SessionTitleEditedEvent(Id.Value, title.Value));
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case SessionSubmittedEvent e: /* set all fields */ break;
            case SessionTitleEditedEvent e: Title = new SessionTitle(e.Title); break;
            // ...
        }
    }
}
```

### 2.4 ISessionRepository anpassen

```csharp
public interface ISessionRepository
{
    Task<Session> GetById(SessionId id);
    Task<IReadOnlyList<Session>> GetSessions(ConferenceId conferenceId);
    Task Save(Session session);
}
```

### 2.5 Session.Domain.UnitTests anpassen

- Tests aktualisieren: Session wird jetzt ueber Factory-Methode `Session.Submit()` erstellt
- Validieren, dass korrekte Events erzeugt werden (`GetUncommittedEvents()`)
- Validieren, dass `ApplyEvent` den Zustand korrekt setzt (via `ReplayEvents()`)

### 2.6 Build und Tests verifizieren

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Session.Domain"` muessen gruen sein.

---

## Schritt 3: Session-Persistence umbauen (Session.Persistence)

### 3.1 EventStore-Referenz hinzufuegen

Projektreferenz auf `ConferenceExample.EventStore` in `Session.Persistence.csproj` hinzufuegen.

### 3.2 SessionRepository umbauen

- `Save()`: Events aus dem Aggregate serialisieren, an den EventStore appenden, dann ueber EventBus publishen, danach `ClearUncommittedEvents()`
- `GetById()`: Events aus dem EventStore laden, deserialisieren, neues Aggregate instanziieren, `ReplayEvents()` aufrufen
- `GetSessions()`: Alle Events laden, nach ConferenceId filtern, Aggregates aufbauen (einfache Loesung fuer den Anfang)

### 3.3 SpeakerRepository umbauen oder entfernen

Pruefen, ob der SpeakerRepository noch benoetigt wird. Falls ja, analog zum SessionRepository umbauen. Falls nicht, entfernen.

### 3.4 SessionExtensions entfernen

Die Mapping-Extensions zwischen Persistence-Models und Domain-Entities werden nicht mehr benoetigt, da Aggregates jetzt direkt ueber Event Replay geladen werden.

### 3.5 Shared-Persistence-Referenz entfernen

Projektreferenz auf `ConferenceExample.Persistence` aus `Session.Persistence.csproj` entfernen.

### 3.6 Build verifizieren

`dotnet build` muss kompilieren. (Tests koennen hier noch fehlschlagen, da die Application-Schicht noch nicht angepasst ist.)

---

## Schritt 4: Session-Application anpassen (Session.Application)

### 4.1 IDatabaseContext entfernen

`IDatabaseContext.cs` aus Session.Application loeschen -- wird nicht mehr benoetigt.

### 4.2 SessionService anpassen

`SessionService` nutzt nur noch `ISessionRepository` statt `IDatabaseContext`. Alle Methoden aktualisieren.

### 4.3 Session.Application.UnitTests anpassen

- `IDatabaseContext`-Mock entfernen
- `ISessionRepository`-Mock verwenden
- Pruefen, dass `Save()` aufgerufen wird

### 4.4 Build und Tests verifizieren

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Session"` muessen gruen sein (Domain + Application UnitTests).

---

## Schritt 5: Conference-Domain umbauen (Conference.Domain)

### 5.1 IDomainEvent und AggregateRoot anlegen

Wie in Session.Domain: `IDomainEvent` und `AggregateRoot` direkt im Conference.Domain-Projekt anlegen (eigenstaendige Duplikation, keine externe Abhaengigkeit).

### 5.2 Conference Domain Events definieren

Events fuer Conference:
- `ConferenceCreatedEvent` -- Name, Time, Location
- `ConferenceRenamedEvent` -- neuer Name

### 5.3 Conference-Aggregate umbauen

`Conference` erbt von `AggregateRoot`. Enthaelt intern eine Liste von Sessions. Alle Mutationen ueber Events. Factory-Methode `Conference.Create(...)`.

### 5.4 Session-Events im Conference-BC definieren

Events fuer die Session-Entitaet innerhalb des Conference-BC:
- `SessionSubmittedToConferenceEvent`
- `SessionAcceptedEvent`
- `SessionRejectedEvent`
- `SessionScheduledEvent`
- `SessionAssignedToRoomEvent`

### 5.5 IConferenceRepository anlegen

```csharp
public interface IConferenceRepository
{
    Task<Conference> GetById(ConferenceId id);
    Task Save(Conference conference);
}
```

### 5.6 Conference.Domain.UnitTests anlegen

- Tests fuer Conference-Aggregate: Erzeugung, Events, State nach Replay
- Tests fuer Session-Lifecycle im Conference-BC (submit, accept, reject, schedule)

### 5.7 Build und Tests verifizieren

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Conference.Domain"` muessen gruen sein.

---

## Schritt 6: Conference-Persistence implementieren (Conference.Persistence)

### 6.1 EventStore-Referenz hinzufuegen

Projektreferenz auf `ConferenceExample.EventStore` in `Conference.Persistence.csproj` hinzufuegen.

### 6.2 ConferenceRepository implementieren

Gleiche Logik wie SessionRepository: Load via Event Replay, Save via Append + Publish.

### 6.3 Platzhalter-Code entfernen

`Class1.cs` aus Conference.Persistence loeschen.

### 6.4 Shared-Persistence-Referenz entfernen

Projektreferenz auf `ConferenceExample.Persistence` aus `Conference.Persistence.csproj` entfernen.

### 6.5 Build verifizieren

`dotnet build` muss kompilieren.

---

## Schritt 7: Conference-Application implementieren (Conference.Application)

### 7.1 ConferenceService implementieren

`IConferenceService` mit `CreateConference`-Methode implementieren. Nutzt `IConferenceRepository`.

### 7.2 Conference.Application.UnitTests anlegen

- Tests fuer ConferenceService
- `IConferenceRepository`-Mock verwenden

### 7.3 Build und Tests verifizieren

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Conference"` muessen gruen sein.

---

## Schritt 8: API-Layer anpassen

### 8.1 EventStore-Referenz hinzufuegen

Projektreferenz auf `ConferenceExample.EventStore` in `ConferenceExample.API.csproj` hinzufuegen.

### 8.2 Persistence-Projekt-Referenzen hinzufuegen

Projektreferenzen auf `Session.Persistence` und `Conference.Persistence` in `ConferenceExample.API.csproj` hinzufuegen (fuer DI-Registrierung der Repositories).

### 8.3 ServiceCollectionExtensions anpassen

- `InMemoryEventStore` als Singleton registrieren
- `InMemoryEventBus` als Singleton registrieren
- `SessionRepository` und `ConferenceRepository` registrieren
- Application Services (`SessionService`, `ConferenceService`) registrieren

### 8.4 Shared-Persistence-Referenz entfernen

Projektreferenz auf `ConferenceExample.Persistence` aus `ConferenceExample.API.csproj` entfernen.

### 8.5 DatabaseContext entfernen

`DatabaseContext.cs` in der API wird nicht mehr benoetigt und kann entfernt werden.

### 8.6 Build verifizieren

`dotnet build` muss kompilieren.

---

## Schritt 9: Cross-BC-Kommunikation ueber EventBus

### 9.1 EventHandler implementieren

Wenn im Session-BC ein `SessionSubmittedEvent` publiziert wird, reagiert ein EventHandler im Conference-BC: Er deserialisiert das `StoredEvent`, laedt das Conference-Aggregate, ruft `SubmitSession()` auf und speichert es.

Der EventHandler arbeitet auf `StoredEvent`-Ebene (String-basierter EventType). Die Deserialisierung in das konkrete Event erfolgt im Handler selbst:

```csharp
eventBus.Subscribe("SessionSubmittedEvent", storedEvent =>
{
    var sessionEvent = JsonSerializer.Deserialize<SessionSubmittedEvent>(storedEvent.Payload);
    var conferenceRepo = serviceProvider.GetRequiredService<IConferenceRepository>();
    var conference = conferenceRepo.GetById(new ConferenceId(sessionEvent.ConferenceId)).Result;
    conference.SubmitSession(new SessionId(storedEvent.AggregateId));
    conferenceRepo.Save(conference).Wait();
});
```

### 9.2 EventBus-Subscriptions registrieren

In `ServiceCollectionExtensions` beim App-Start die Subscriptions einrichten. Dies geschieht nach der Registrierung aller Services.

### 9.3 Build verifizieren

`dotnet build` muss kompilieren.

---

## Schritt 10: Shared-Persistence-Projekt entfernen

### 10.1 Alle Referenzen pruefen

Sicherstellen, dass kein Projekt mehr `ConferenceExample.Persistence` referenziert. Betrifft auch Testprojekte (insbesondere `Session.AcceptanceTests`).

### 10.2 AcceptanceTests-Referenz anpassen

`ConferenceExample.Persistence`-Referenz aus `Session.AcceptanceTests.csproj` entfernen. DI-Setup in den AcceptanceTests auf EventStore umstellen.

### 10.3 Projekt aus Solution entfernen und loeschen

- `ConferenceExample.Persistence` aus der Solution entfernen (`dotnet sln remove`)
- Projektordner loeschen

### 10.4 Build verifizieren

`dotnet build` muss kompilieren.

---

## Schritt 11: Architecture Tests anpassen

### 11.1 Dependencies aktualisieren

- `Persistence`-Assembly-Referenz (shared) entfernen
- Neue `EventStore`-Assembly-Referenz hinzufuegen

### 11.2 Dependency Rules aktualisieren

- Domain Dependency Rules bleiben gleich (Domain haengt weiterhin nur von sich selbst + System ab)
- Persistence Dependency Rules aktualisieren: `EventStore` statt shared `Persistence`
- Application Dependency Rules bleiben gleich (Application haengt von Domain ab, nicht von Persistence/EventStore)

### 11.3 Architecture Tests ausfuehren

`dotnet test --filter "FullyQualifiedName~Architecture"` muss gruen sein.

---

## Schritt 12: Acceptance Tests anpassen

### 12.1 DI-Setup aktualisieren

In `SetupTestDependencies.cs`: `InMemoryEventStore` und `InMemoryEventBus` registrieren, Repositories registrieren, `DatabaseContext` entfernen.

### 12.2 Acceptance Tests ausfuehren

`dotnet test --filter "FullyQualifiedName~AcceptanceTests"` muss gruen sein.

---

## Schritt 13: Alle Tests gruen

### 13.1 Gesamten Build und alle Tests ausfuehren

```bash
dotnet build
dotnet test
```

Alle Tests muessen gruen sein. Keine Warnings (TreatWarningsAsErrors ist aktiv).

---

## Reihenfolge der Umsetzung

Jeder Schritt baut auf den vorherigen auf. Nach jedem Schritt wird der Build (und wo moeglich die Tests) verifiziert.

| # | Schritt | Baut auf |
|---|---------|----------|
| 1 | EventStore-Projekt anlegen (StoredEvent, IEventStore, InMemoryEventStore, IEventBus, InMemoryEventBus) | -- |
| 2 | Session-Domain umbauen (IDomainEvent, AggregateRoot, Events, Aggregate) + UnitTests anpassen | 1 |
| 3 | Session-Persistence umbauen (Repository mit EventStore, Shared-Persistence-Referenz entfernen) | 1, 2 |
| 4 | Session-Application anpassen (IDatabaseContext entfernen, SessionService anpassen) + UnitTests anpassen | 3 |
| 5 | Conference-Domain umbauen (IDomainEvent, AggregateRoot, Events, Aggregate) + UnitTests anlegen | 1 |
| 6 | Conference-Persistence implementieren (Repository mit EventStore, Shared-Persistence-Referenz entfernen) | 1, 5 |
| 7 | Conference-Application implementieren (ConferenceService) + UnitTests anlegen | 6 |
| 8 | API-Layer anpassen (DI, EventStore/Repos/Services registrieren, Shared-Persistence-Referenz + DatabaseContext entfernen) | 4, 7 |
| 9 | Cross-BC-Kommunikation (EventHandler + Subscriptions) | 8 |
| 10 | Shared-Persistence-Projekt entfernen (AcceptanceTests anpassen, Projekt aus Solution loeschen) | 8 |
| 11 | Architecture Tests anpassen | 10 |
| 12 | Acceptance Tests anpassen | 10 |
| 13 | Alle Tests gruen (Gesamtbuild + alle Tests) | 11, 12 |

**Hinweis:** Schritte 2-4 (Session-BC) und 5-7 (Conference-BC) sind untereinander unabhaengig und koennten theoretisch parallel bearbeitet werden. Die lineare Reihenfolge ist empfohlen, damit Erkenntnisse aus dem Session-BC-Umbau in den Conference-BC einfliessen.

---

## Offene Entscheidungen (spaeter)

- **ReadModels**: Separate Projektionen mit eigener Datenbank -- werden vorerst nicht eingefuehrt
- **Datenbanktechnologie** fuer den EventStore (PostgreSQL, EventStoreDB, etc.)
- **Snapshotting** fuer Aggregates mit vielen Events
- **Async Event Processing** -- aktuell synchron ueber Observer Pattern
- **Idempotenz** der Event Handler
