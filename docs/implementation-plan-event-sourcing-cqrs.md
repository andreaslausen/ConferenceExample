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

## Schritt 1: Zentrales EventStore-Projekt anlegen

Neues Projekt `ConferenceExample.EventStore` erstellen. Dieses Projekt enthaelt die Infrastruktur fuer Event-Persistierung und den EventBus. Es kennt keine Domain-Events direkt -- es arbeitet mit serialisierten Events (`StoredEvent`).

### 1.1 StoredEvent und IEventStore

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

### 1.2 EventBus (Observer Pattern)

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

### 1.3 IDomainEvent und AggregateRoot (pro Domain-Projekt dupliziert)

Diese Typen werden **nicht** im EventStore-Projekt definiert, sondern in jedem Domain-Projekt eigenstaendig:

**`IDomainEvent`** -- Marker-Interface:
```csharp
public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccurredAt { get; }
}
```

**`AggregateRoot`** -- Basisklasse:
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

Dadurch bleibt jedes Domain-Projekt frei von externen Abhaengigkeiten. Die Persistence-Projekte uebernehmen die Serialisierung/Deserialisierung zwischen den Domain-Events und `StoredEvent`.

---

## Schritt 2: Session Bounded Context umbauen

### 2.1 Domain Events definieren (Session.Domain)

`IDomainEvent` und `AggregateRoot` werden direkt im Session.Domain-Projekt angelegt (keine externe Abhaengigkeit).

Events fuer die Session-Entitaet:
- `SessionSubmittedEvent` -- enthaelt alle initialen Daten (Title, Abstract, SpeakerId, Tags, SessionTypeId, ConferenceId)
- `SessionTitleEditedEvent` -- enthaelt neuen Title
- `SessionAbstractEditedEvent` -- enthaelt neues Abstract
- `SessionTagAddedEvent` -- enthaelt Tag
- `SessionTagRemovedEvent` -- enthaelt Tag

### 2.2 Session-Aggregate umbauen (Session.Domain)

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

### 2.3 ISessionRepository anpassen (Session.Domain)

```csharp
public interface ISessionRepository
{
    Task<Session> GetById(SessionId id);
    Task<IReadOnlyList<Session>> GetSessions(ConferenceId conferenceId);
    Task Save(Session session);
}
```

### 2.4 SessionRepository umbauen (Session.Persistence)

- Abhaengigkeit auf `ConferenceExample.EventStore` statt `ConferenceExample.Persistence`
- `Save()`: Events aus dem Aggregate an den EventStore appenden, dann ueber EventBus publishen
- `GetById()`: Events aus dem EventStore laden, neues Aggregate instanziieren, `ReplayEvents()` aufrufen
- `GetSessions()`: Alle Events laden, nach ConferenceId filtern, Aggregates aufbauen (einfache Loesung fuer den Anfang)

### 2.5 Application Layer anpassen (Session.Application)

- `IDatabaseContext` aus Session.Application **entfernen** (wird nicht mehr benoetigt)
- `SessionService` vereinfachen: nutzt nur noch `ISessionRepository`
- Spaeter Aufspaltung in dedizierte CommandHandler/QueryHandler moeglich, aber fuers Erste reicht der Service

---

## Schritt 3: Conference Bounded Context umbauen

### 3.1 Domain Events definieren (Conference.Domain)

`IDomainEvent` und `AggregateRoot` werden direkt im Conference.Domain-Projekt angelegt (keine externe Abhaengigkeit, Duplikation wie im Session-BC).

Events fuer Conference:
- `ConferenceCreatedEvent` -- Name, Time, Location
- `ConferenceRenamedEvent` -- neuer Name

Events fuer Conference.Session (die Session-Entitaet im Conference-BC):
- `SessionSubmittedToConferenceEvent`
- `SessionAcceptedEvent`
- `SessionRejectedEvent`
- `SessionScheduledEvent`
- `SessionAssignedToRoomEvent`

### 3.2 Conference-Aggregate umbauen (Conference.Domain)

`Conference` erbt von `AggregateRoot`. Enthaelt intern eine Liste von Sessions. Alle Mutationen ueber Events.

### 3.3 Repository-Interfaces anlegen (Conference.Domain)

```csharp
public interface IConferenceRepository
{
    Task<Conference> GetById(ConferenceId id);
    Task Save(Conference conference);
}
```

### 3.4 ConferenceRepository implementieren (Conference.Persistence)

- Abhaengigkeit auf `ConferenceExample.EventStore`
- Gleiche Logik wie SessionRepository: Load via Replay, Save via Append + Publish

### 3.5 Application Layer (Conference.Application)

- `IConferenceService` mit `CreateConference`-Methode implementieren
- Nutzt `IConferenceRepository`

---

## Schritt 4: Cross-BC-Kommunikation ueber EventBus

### 4.1 Beispiel: Session eingereicht im Session-BC -> Conference-BC reagiert

Wenn im Session-BC ein `SessionSubmittedEvent` publiziert wird, kann ein EventHandler im Conference-BC darauf reagieren und eine `Session`-Entitaet im Conference-Aggregate anlegen (`SessionSubmittedToConferenceEvent`).

### 4.2 EventHandler registrieren

In `ServiceCollectionExtensions` beim App-Start die Subscriptions einrichten:

```csharp
eventBus.Subscribe<SessionSubmittedEvent>(async e =>
{
    var conferenceRepo = serviceProvider.GetRequiredService<IConferenceRepository>();
    var conference = await conferenceRepo.GetById(new ConferenceId(e.ConferenceId));
    conference.SubmitSession(new SessionId(e.AggregateId));
    await conferenceRepo.Save(conference);
});
```

---

## Schritt 5: Shared Persistence Projekt entfernen

### 5.1 Abhaengigkeiten aufloesen

- `ConferenceExample.API` referenziert nicht mehr `ConferenceExample.Persistence`, sondern `ConferenceExample.EventStore`
- `DatabaseContext` in der API wird durch EventStore/EventBus-Registrierung ersetzt
- `Session.Persistence` referenziert `ConferenceExample.EventStore` statt `ConferenceExample.Persistence`
- `Conference.Persistence` referenziert `ConferenceExample.EventStore` statt `ConferenceExample.Persistence`

### 5.2 Projekt entfernen

- `ConferenceExample.Persistence` aus der Solution entfernen
- Projektordner loeschen

---

## Schritt 6: API-Layer anpassen

### 6.1 ServiceCollectionExtensions

- `InMemoryEventStore` als Singleton registrieren
- `InMemoryEventBus` als Singleton registrieren
- Repositories registrieren
- Application Services registrieren
- EventBus-Subscriptions fuer Cross-BC-Kommunikation einrichten

### 6.2 DatabaseContext entfernen

`DatabaseContext.cs` in der API wird nicht mehr benoetigt und kann entfernt werden.

---

## Schritt 7: Architecture Tests anpassen

### 7.1 Neue Abhaengigkeiten abbilden

- Domain-Projekte haben **keine** neue externe Abhaengigkeit (IDomainEvent und AggregateRoot sind dupliziert)
- Persistence-Projekte duerfen auf `ConferenceExample.EventStore` zugreifen (fuer `IEventStore`, `IEventBus`, `StoredEvent`)
- `ConferenceExample.Persistence` Assembly-Referenz entfernen

### 7.2 ArchitectureTest.cs anpassen

- `Persistence`-Assembly-Referenz (shared) entfernen
- Neue `EventStore`-Assembly-Referenz hinzufuegen
- Domain Dependency Rules bleiben gleich (Domain haengt weiterhin nur von sich selbst + System ab)
- Persistence Dependency Rules aktualisieren: `EventStore` statt shared `Persistence`

---

## Schritt 8: Bestehende Tests anpassen

### 8.1 Unit Tests (Session.Domain.UnitTests)

- Tests anpassen: Session wird jetzt ueber Factory-Methode `Session.Submit()` erstellt
- Validieren, dass korrekte Events erzeugt werden
- Validieren, dass `ApplyEvent` den Zustand korrekt setzt

### 8.2 Application Unit Tests (Session.Application.UnitTests)

- `IDatabaseContext`-Mock entfernen
- `ISessionRepository`-Mock beibehalten
- Pruefen, dass `Save()` aufgerufen wird

### 8.3 Acceptance Tests (Session.AcceptanceTests)

- DI-Setup anpassen: `InMemoryEventStore` und `InMemoryEventBus` registrieren
- `DatabaseContext` durch EventStore ersetzen

---

## Reihenfolge der Umsetzung

| # | Schritt | Abhaengigkeit |
|---|---------|---------------|
| 1 | EventStore-Projekt anlegen (Interfaces, InMemory, EventBus, AggregateRoot) | -- |
| 2 | Session-Domain umbauen (Events, Aggregate mit AggregateRoot) | 1 |
| 3 | Session-Persistence umbauen (Repository mit EventStore) | 1, 2 |
| 4 | Session-Application anpassen (IDatabaseContext entfernen) | 3 |
| 5 | Conference-Domain umbauen (Events, Aggregate mit AggregateRoot) | 1 |
| 6 | Conference-Persistence implementieren (Repository mit EventStore) | 1, 5 |
| 7 | Conference-Application implementieren | 6 |
| 8 | Cross-BC EventHandler einrichten | 4, 7 |
| 9 | Shared Persistence Projekt entfernen | 3, 6 |
| 10 | API-Layer anpassen (DI, Controller, alten DatabaseContext entfernen) | 4, 7, 8, 9 |
| 11 | Architecture Tests anpassen | 9, 10 |
| 12 | Unit Tests + Acceptance Tests anpassen | 2-10 |
| 13 | Build + alle Tests gruen | 12 |

---

## Offene Entscheidungen (spaeter)

- **ReadModels**: Separate Projektionen mit eigener Datenbank -- werden vorerst nicht eingefuehrt
- **Datenbanktechnologie** fuer den EventStore (PostgreSQL, EventStoreDB, etc.)
- **Snapshotting** fuer Aggregates mit vielen Events
- **Async Event Processing** -- aktuell synchron ueber Observer Pattern
- **Idempotenz** der Event Handler
