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

## Schritt 2: Session-Domain umbauen (Session.Domain) ✅

### 2.1 IDomainEvent und AggregateRoot anlegen ✅

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

### 2.2 Domain Events definieren ✅

Events fuer die Session-Entitaet:
- `SessionSubmittedEvent` -- enthaelt alle initialen Daten (Title, Abstract, SpeakerId, Tags, SessionTypeId, ConferenceId)
- `SessionTitleEditedEvent` -- enthaelt neuen Title
- `SessionAbstractEditedEvent` -- enthaelt neues Abstract
- `SessionTagAddedEvent` -- enthaelt Tag
- `SessionTagRemovedEvent` -- enthaelt Tag

### 2.3 Session-Aggregate umbauen ✅

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

### 2.4 ISessionRepository anpassen ✅

```csharp
public interface ISessionRepository
{
    Task<Session> GetById(SessionId id);
    Task<IReadOnlyList<Session>> GetSessions(ConferenceId conferenceId);
    Task Save(Session session);
}
```

### 2.5 Session.Domain.UnitTests anpassen ✅

- Tests aktualisieren: Session wird jetzt ueber Factory-Methode `Session.Submit()` erstellt
- Validieren, dass korrekte Events erzeugt werden (`GetUncommittedEvents()`)
- Validieren, dass `ApplyEvent` den Zustand korrekt setzt (via `ReplayEvents()`)

### 2.6 Build und Tests verifizieren ✅

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Session.Domain"` muessen gruen sein.

---

## Schritt 3: Session-Persistence umbauen (Session.Persistence) ✅

### 3.1 EventStore-Referenz hinzufuegen ✅

Projektreferenz auf `ConferenceExample.EventStore` in `Session.Persistence.csproj` hinzufuegen.

### 3.2 SessionRepository umbauen ✅

- `Save()`: Events aus dem Aggregate serialisieren, an den EventStore appenden, dann ueber EventBus publishen, danach `ClearUncommittedEvents()`
- `GetById()`: Events aus dem EventStore laden, deserialisieren, neues Aggregate instanziieren, `ReplayEvents()` aufrufen
- `GetSessions()`: Alle Events laden, nach ConferenceId filtern, Aggregates aufbauen (einfache Loesung fuer den Anfang)

### 3.3 SpeakerRepository umbauen oder entfernen ✅

Pruefen, ob der SpeakerRepository noch benoetigt wird. Falls ja, analog zum SessionRepository umbauen. Falls nicht, entfernen.

### 3.4 SessionExtensions entfernen ✅

Die Mapping-Extensions zwischen Persistence-Models und Domain-Entities werden nicht mehr benoetigt, da Aggregates jetzt direkt ueber Event Replay geladen werden.

### 3.5 Shared-Persistence-Referenz entfernen ✅

Projektreferenz auf `ConferenceExample.Persistence` aus `Session.Persistence.csproj` entfernen.

### 3.6 Build verifizieren ✅

`dotnet build` muss kompilieren. (Tests koennen hier noch fehlschlagen, da die Application-Schicht noch nicht angepasst ist.)

---

## Schritt 4: Session-Application anpassen (Session.Application) ✅

### 4.1 IDatabaseContext entfernen ✅

`IDatabaseContext.cs` aus Session.Application loeschen -- wird nicht mehr benoetigt.

### 4.2 SessionService anpassen ✅

`SessionService` nutzt nur noch `ISessionRepository` statt `IDatabaseContext`. Alle Methoden aktualisieren.

### 4.3 Session.Application.UnitTests anpassen ✅

- `IDatabaseContext`-Mock entfernen
- `ISessionRepository`-Mock verwenden
- Pruefen, dass `Save()` aufgerufen wird

### 4.4 Build und Tests verifizieren ✅

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Session"` muessen gruen sein (Domain + Application UnitTests).

---

## Schritt 5: Conference-Domain umbauen (Conference.Domain) ✅

### 5.1 IDomainEvent und AggregateRoot anlegen ✅

Wie in Session.Domain: `IDomainEvent` und `AggregateRoot` direkt im Conference.Domain-Projekt anlegen (eigenstaendige Duplikation, keine externe Abhaengigkeit).

### 5.2 Conference Domain Events definieren ✅

Events fuer Conference:
- `ConferenceCreatedEvent` -- Name, Time, Location
- `ConferenceRenamedEvent` -- neuer Name

### 5.3 Conference-Aggregate umbauen ✅

`Conference` erbt von `AggregateRoot`. Enthaelt intern eine Liste von Sessions. Alle Mutationen ueber Events. Factory-Methode `Conference.Create(...)`.

### 5.4 Session-Events im Conference-BC definieren ✅

Events fuer die Session-Entitaet innerhalb des Conference-BC:
- `SessionSubmittedToConferenceEvent`
- `SessionAcceptedEvent`
- `SessionRejectedEvent`
- `SessionScheduledEvent`
- `SessionAssignedToRoomEvent`

### 5.5 IConferenceRepository anlegen ✅

```csharp
public interface IConferenceRepository
{
    Task<Conference> GetById(ConferenceId id);
    Task Save(Conference conference);
}
```

### 5.6 Conference.Domain.UnitTests anlegen ✅

- Tests fuer Conference-Aggregate: Erzeugung, Events, State nach Replay
- Tests fuer Session-Lifecycle im Conference-BC (submit, accept, reject, schedule)

### 5.7 Build und Tests verifizieren ✅

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Conference.Domain"` muessen gruen sein.

---

## Schritt 6: Fehlende Unit Tests nachholen (EventStore, Persistence, Value Objects) ✅

Bevor es mit der Conference-Persistence weitergeht, werden alle Unit Tests nachgeholt, die in den bisherigen Schritten nicht beruecksichtigt waren. So ist die bestehende Codebasis vollstaendig abgedeckt, bevor neuer Code entsteht.

### 6.1 EventStore Unit Tests anlegen ✅

Neues Testprojekt `ConferenceExample.EventStore.UnitTests` erstellen und in die Solution einbinden. Referenz auf `ConferenceExample.EventStore`.

**InMemoryEventStore Tests:**
- `AppendEvents_StoresEvents_GetEventsReturnsThem` -- Events speichern und per `GetEvents` abrufen ✅
- `GetEvents_UnknownAggregate_ReturnsEmptyList` -- unbekannte AggregateId liefert leere Liste ✅
- `GetEvents_ReturnsEventsOrderedByVersion` -- Events sind nach Version sortiert ✅
- `GetAllEvents_ReturnsEventsAcrossMultipleAggregates` -- alle Events aller Aggregates ✅
- `AppendEvents_VersionMismatch_ThrowsConcurrencyException` -- Optimistic Concurrency: falscher `expectedVersion` wirft `ConcurrencyException` ✅
- `AppendEvents_MultipleAggregates_KeepsEventsSeparate` -- Events verschiedener Aggregates sind unabhaengig ✅

**InMemoryEventBus Tests:**
- `Publish_SubscribedHandler_ReceivesEvent` -- abonnierter Handler wird aufgerufen ✅
- `Publish_NoSubscribers_DoesNotThrow` -- kein Subscriber fuer EventType wirft nicht ✅
- `Publish_MultipleSubscribers_AllReceiveEvent` -- mehrere Handler fuer gleichen EventType werden alle aufgerufen ✅
- `Publish_DifferentEventTypes_OnlyMatchingSubscribersReceive` -- Handler nur fuer passenden EventType ✅

### 6.2 Session.Persistence Unit Tests anlegen ✅

Neues Testprojekt `ConferenceExample.Session.Persistence.UnitTests` erstellen und in die Solution einbinden. Referenzen auf `Session.Persistence`, `Session.Domain`, `ConferenceExample.EventStore`.

**SessionRepository Tests:**
- `Save_NewSession_AppendsSerializedEventsToEventStore` -- Events werden serialisiert und im EventStore gespeichert ✅
- `Save_NewSession_PublishesEventsToEventBus` -- Events werden ueber den EventBus publiziert ✅
- `Save_ClearsUncommittedEventsAfterSaving` -- nach Save sind keine uncommitted Events mehr vorhanden ✅
- `Save_NoUncommittedEvents_DoesNothing` -- Session ohne uncommitted Events fuehrt zu keinem EventStore-Aufruf ✅
- `GetById_ExistingSession_RebuildsSessionFromEvents` -- Events laden, deserialisieren, Session per Replay aufbauen ✅
- `GetById_UnknownSession_ThrowsInvalidOperationException` -- unbekannte SessionId wirft Exception ✅
- `GetSessions_FiltersSessionsByConferenceId` -- nur Sessions der angegebenen ConferenceId werden zurueckgegeben ✅
- `GetSessions_NoMatchingSessions_ReturnsEmptyList` -- keine passenden Sessions liefert leere Liste ✅

### 6.3 Session.Domain: Fehlende Event-Assertions ergaenzen ✅

Folgende Tests in `SessionTests.cs` hinzugefuegt:

- `EditAbstract_RaisesSessionAbstractEditedEvent` -- validiert, dass das korrekte Event erzeugt wird ✅
- `AddTag_RaisesSessionTagAddedEvent` -- validiert, dass das korrekte Event erzeugt wird ✅
- `RemoveTag_RaisesSessionTagRemovedEvent` -- validiert, dass das korrekte Event erzeugt wird ✅

### 6.4 Session.Domain: Value Object Tests anlegen ✅

Neue Testklassen in `Session.Domain.UnitTests` fuer Value Objects:

**SessionTitleTests:** ✅
- `Constructor_ValidTitle_SetsProperty` -- gueltiger Title wird gesetzt ✅
- `Constructor_TitleExceeds100Characters_ThrowsArgumentException` -- zu langer Title wirft Exception ✅
- `Constructor_TitleExactly100Characters_DoesNotThrow` -- Grenzwert wird akzeptiert ✅

**AbstractTests:** ✅
- `Constructor_ValidAbstract_SetsProperty` -- gueltiges Abstract wird gesetzt ✅
- `Constructor_AbstractExceeds1000Characters_ThrowsArgumentException` -- zu langes Abstract wirft Exception ✅
- `Constructor_AbstractExactly1000Characters_DoesNotThrow` -- Grenzwert wird akzeptiert ✅

**SessionTagTests:** ✅
- `Constructor_ValidTag_SetsProperty` -- gueltiger Tag wird gesetzt ✅
- `Constructor_TagExceeds20Characters_ThrowsArgumentException` -- zu langer Tag wirft Exception ✅
- `Constructor_TagExactly20Characters_DoesNotThrow` -- Grenzwert wird akzeptiert ✅

### 6.5 Conference.Domain: Value Object Tests anlegen ✅

Neue Testklasse in `Conference.Domain.UnitTests`:

**TextTests:** ✅
- `Constructor_ValidText_SetsProperty` -- gueltiger Text wird gesetzt ✅
- `Constructor_NullValue_ThrowsArgumentException` -- null wirft Exception ✅
- `Constructor_EmptyString_ThrowsArgumentException` -- leerer String wirft Exception ✅
- `Constructor_WhitespaceOnly_ThrowsArgumentException` -- Whitespace wirft Exception ✅

### 6.6 Conference.Domain: Fehlende Event-Assertions ergaenzen ✅

Folgende Tests in `ConferenceTests.cs` hinzugefuegt:

- `AcceptSession_RaisesSessionAcceptedEvent` -- validiert, dass das korrekte Event erzeugt wird ✅
- `RejectSession_RaisesSessionRejectedEvent` -- validiert, dass das korrekte Event erzeugt wird ✅
- `ScheduleSession_RaisesSessionScheduledEvent` -- validiert, dass das korrekte Event erzeugt wird ✅
- `AssignSessionToRoom_RaisesSessionAssignedToRoomEvent` -- validiert, dass das korrekte Event erzeugt wird ✅

### 6.7 Platzhalter-Tests entfernen ✅

`Conference.UnitTests` war bereits nicht in der Solution eingebunden -- kein Handlungsbedarf.

### 6.8 Build und alle Tests verifizieren ✅

```bash
dotnet build
dotnet test
```

Alle Unit Tests gruen (59 Tests). Die 2 Fehler in ArchitectureTests sind pre-existing und werden in Schritt 12 behoben.

---

## Schritt 7: Conference-Persistence implementieren (Conference.Persistence) ✅

### 7.1 EventStore-Referenz hinzufuegen ✅

Projektreferenz auf `ConferenceExample.EventStore` in `Conference.Persistence.csproj` hinzufuegen.

### 7.2 ConferenceRepository implementieren ✅

Gleiche Logik wie SessionRepository: Load via Event Replay, Save via Append + Publish.

### 7.3 Platzhalter-Code entfernen ✅

`Class1.cs` aus Conference.Persistence loeschen.

### 7.4 Shared-Persistence-Referenz entfernen ✅

Projektreferenz auf `ConferenceExample.Persistence` aus `Conference.Persistence.csproj` entfernen.

### 7.5 Conference.Persistence Unit Tests anlegen ✅

Neues Testprojekt `ConferenceExample.Conference.Persistence.UnitTests` erstellen und in die Solution einbinden. Referenzen auf `Conference.Persistence`, `Conference.Domain`, `ConferenceExample.EventStore`.

**ConferenceRepository Tests:**
- `Save_NewConference_AppendsSerializedEventsToEventStore` -- Events werden serialisiert und im EventStore gespeichert ✅
- `Save_NewConference_PublishesEventsToEventBus` -- Events werden ueber den EventBus publiziert ✅
- `Save_ClearsUncommittedEventsAfterSaving` -- nach Save sind keine uncommitted Events mehr vorhanden ✅
- `Save_NoUncommittedEvents_DoesNothing` -- Conference ohne uncommitted Events fuehrt zu keinem EventStore-Aufruf ✅
- `GetById_ExistingConference_RebuildsConferenceFromEvents` -- Events laden, deserialisieren, Conference per Replay aufbauen ✅
- `GetById_UnknownConference_ThrowsInvalidOperationException` -- unbekannte ConferenceId wirft Exception ✅

### 7.6 Build und Tests verifizieren ✅

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Conference.Persistence"` gruen (6/6 Tests bestanden).

---

## Schritt 8: Conference-Application implementieren (Conference.Application) ✅

### 8.1 ConferenceService implementieren ✅

`IConferenceService` mit `CreateConference`-Methode implementieren. Nutzt `IConferenceRepository`.

### 8.2 Conference.Application.UnitTests anlegen ✅

**ConferenceServiceTests:**
- `CreateConference_ValidDto_CallsRepositorySave` -- `Save()` wird mit korrekt erzeugtem Aggregate aufgerufen ✅
- `CreateConference_ValidDto_CreatesConferenceWithCorrectProperties` -- das gespeicherte Aggregate hat die richtigen Werte aus dem DTO ✅

### 8.3 Build und Tests verifizieren ✅

`dotnet build` und `dotnet test --filter "FullyQualifiedName~Conference.Application"` gruen (2/2 Tests bestanden).

---

## Schritt 9: API-Layer anpassen ✅

### 9.1 EventStore-Referenz hinzufuegen ✅

Projektreferenz auf `ConferenceExample.EventStore` in `ConferenceExample.API.csproj` hinzufuegen.

### 9.2 Persistence-Projekt-Referenzen hinzufuegen ✅

Projektreferenzen auf `Session.Persistence` und `Conference.Persistence` in `ConferenceExample.API.csproj` hinzufuegen (fuer DI-Registrierung der Repositories).

### 9.3 ServiceCollectionExtensions anpassen ✅

- `InMemoryEventStore` als Singleton registrieren
- `InMemoryEventBus` als Singleton registrieren
- `SessionRepository` und `ConferenceRepository` registrieren
- Application Services (`SessionService`, `ConferenceService`) registrieren

### 9.4 Shared-Persistence-Referenz entfernen ✅

Projektreferenz auf `ConferenceExample.Persistence` aus `ConferenceExample.API.csproj` entfernen.

### 9.5 DatabaseContext entfernen ✅

`DatabaseContext.cs` in der API wird nicht mehr benoetigt und kann entfernt werden.

Hinweis: Das Loeschen von `DatabaseContext.cs` hat zwei weitere Dateien gebrochen, die minimale Korrekturen benoetigt haben:
- `ArchitectureTest.cs`: `typeof(DatabaseContext)` ersetzt durch `typeof(ConferenceExample.API.Extensions.ServiceCollectionExtensions)`
- `SetupTestDependencies.cs` (AcceptanceTests): DI-Setup von `DatabaseContext`/`IDatabaseContext` auf `InMemoryEventStore`/`InMemoryEventBus` umgestellt (Persistence-Projektreferenz bleibt noch -- wird in Schritt 11 entfernt)

### 9.6 Build verifizieren ✅

`dotnet build` kompiliert erfolgreich (0 Fehler, 0 Warnungen).

---

## Schritt 10: Cross-BC-Kommunikation ueber EventBus

### 10.1 EventHandler implementieren

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

### 10.2 EventBus-Subscriptions registrieren

In `ServiceCollectionExtensions` beim App-Start die Subscriptions einrichten. Dies geschieht nach der Registrierung aller Services.

### 10.3 Build verifizieren

`dotnet build` muss kompilieren.

---

## Schritt 11: Shared-Persistence-Projekt entfernen

### 11.1 Alle Referenzen pruefen

Sicherstellen, dass kein Projekt mehr `ConferenceExample.Persistence` referenziert. Betrifft auch Testprojekte (insbesondere `Session.AcceptanceTests`).

### 11.2 AcceptanceTests-Referenz anpassen

`ConferenceExample.Persistence`-Referenz aus `Session.AcceptanceTests.csproj` entfernen. DI-Setup in den AcceptanceTests auf EventStore umstellen.

### 11.3 Projekt aus Solution entfernen und loeschen

- `ConferenceExample.Persistence` aus der Solution entfernen (`dotnet sln remove`)
- Projektordner loeschen

### 11.4 Build verifizieren

`dotnet build` muss kompilieren.

---

## Schritt 12: Architecture Tests anpassen

### 12.1 Dependencies aktualisieren

- `Persistence`-Assembly-Referenz (shared) entfernen
- Neue `EventStore`-Assembly-Referenz hinzufuegen

### 12.2 Dependency Rules aktualisieren

- Domain Dependency Rules bleiben gleich (Domain haengt weiterhin nur von sich selbst + System ab)
- Persistence Dependency Rules aktualisieren: `EventStore` statt shared `Persistence`
- Application Dependency Rules bleiben gleich (Application haengt von Domain ab, nicht von Persistence/EventStore)

### 12.3 Architecture Tests ausfuehren

`dotnet test --filter "FullyQualifiedName~Architecture"` muss gruen sein.

---

## Schritt 13: Acceptance Tests anpassen

### 13.1 DI-Setup aktualisieren

In `SetupTestDependencies.cs`: `InMemoryEventStore` und `InMemoryEventBus` registrieren, Repositories registrieren, `DatabaseContext` entfernen.

### 13.2 Acceptance Tests ausfuehren

`dotnet test --filter "FullyQualifiedName~AcceptanceTests"` muss gruen sein.

---

## Schritt 14: Alle Tests gruen

### 14.1 Gesamten Build und alle Tests ausfuehren

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
| **6** ✅ | **Fehlende Unit Tests nachholen: EventStore, Session.Persistence, Value Objects, Event-Assertions** | **1-5** |
| **7** ✅ | **Conference-Persistence implementieren (Repository mit EventStore) + UnitTests anlegen** | **1, 5, 6** |
| **8** ✅ | **Conference-Application implementieren (ConferenceService) + UnitTests anlegen** | **7** |
| **9** ✅ | **API-Layer anpassen (DI, EventStore/Repos/Services registrieren, Shared-Persistence-Referenz + DatabaseContext entfernen)** | **4, 8** |
| 10 | Cross-BC-Kommunikation (EventHandler + Subscriptions) | 9 |
| 11 | Shared-Persistence-Projekt entfernen (AcceptanceTests anpassen, Projekt aus Solution loeschen) | 9 |
| 12 | Architecture Tests anpassen | 11 |
| 13 | Acceptance Tests anpassen | 11 |
| 14 | Alle Tests gruen (Gesamtbuild + alle Tests) | 12, 13 |

**Hinweis:** Schritte 2-4 (Session-BC) und 5 (Conference-Domain) sind untereinander unabhaengig und koennten theoretisch parallel bearbeitet werden. Die lineare Reihenfolge ist empfohlen, damit Erkenntnisse aus dem Session-BC-Umbau in den Conference-BC einfliessen.

---

## Offene Entscheidungen (spaeter)

- **ReadModels**: Separate Projektionen mit eigener Datenbank -- werden vorerst nicht eingefuehrt
- **Datenbanktechnologie** fuer den EventStore (PostgreSQL, EventStoreDB, etc.)
- **Snapshotting** fuer Aggregates mit vielen Events
- **Async Event Processing** -- aktuell synchron ueber Observer Pattern
- **Idempotenz** der Event Handler
