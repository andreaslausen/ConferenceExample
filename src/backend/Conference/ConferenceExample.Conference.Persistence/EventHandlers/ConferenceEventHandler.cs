using System.Text.Json;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Updates Conference Read Models in response to slim Conference domain events.
/// Each handler applies only the delta carried by its event.
/// </summary>
public class ConferenceEventHandler
{
    private readonly IConferenceDocumentRepository _readModelRepository;

    public ConferenceEventHandler(IConferenceDocumentRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    public async Task HandleConferenceCreated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<ConferenceCreatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var newReadModel = new ConferenceDocument
        {
            Id = storedEvent.AggregateId.ToString(),
            Name = payload.Name,
            Start = payload.Start,
            End = payload.End,
            LocationName = payload.LocationName,
            Street = payload.Street,
            City = payload.City,
            State = payload.State,
            PostalCode = payload.PostalCode,
            Country = payload.Country,
            OrganizerId = payload.OrganizerId.ToString(),
            Status = payload.Status,
            CreatedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
            Version = storedEvent.Version,
        };

        await _readModelRepository.Save(newReadModel);
    }

    public async Task HandleConferenceRenamed(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<RenamedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Name = payload.Name;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleConferenceStatusChanged(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<StatusChangedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Status = payload.Status;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleConferenceDetailsUpdated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<DetailsUpdatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Name = payload.Name;
        readModel.Start = payload.Start;
        readModel.End = payload.End;
        readModel.LocationName = payload.LocationName;
        readModel.Street = payload.Street;
        readModel.City = payload.City;
        readModel.State = payload.State;
        readModel.PostalCode = payload.PostalCode;
        readModel.Country = payload.Country;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkTypeDefined(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkTypeDefinedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        var talkTypeId = payload.TalkTypeId.ToString();
        if (readModel.TalkTypes.All(tt => tt.Id != talkTypeId))
        {
            readModel.TalkTypes.Add(
                new ConferenceDocument.TalkTypeDocument
                {
                    Id = talkTypeId,
                    Name = payload.TalkTypeName,
                    DurationInMinutes = payload.TalkTypeDurationInMinutes,
                }
            );
        }

        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkTypeRemoved(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkTypeRemovedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        var talkTypeId = payload.TalkTypeId.ToString();
        readModel.TalkTypes.RemoveAll(tt => tt.Id == talkTypeId);

        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    private record ConferenceCreatedPayload(
        string Name,
        DateTimeOffset Start,
        DateTimeOffset End,
        string LocationName,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        Guid OrganizerId,
        string Status
    );

    private record RenamedPayload(string Name);

    private record StatusChangedPayload(string Status);

    private record DetailsUpdatedPayload(
        string Name,
        DateTimeOffset Start,
        DateTimeOffset End,
        string LocationName,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country
    );

    private record TalkTypeDefinedPayload(
        Guid TalkTypeId,
        string TalkTypeName,
        int TalkTypeDurationInMinutes
    );

    private record TalkTypeRemovedPayload(Guid TalkTypeId);
}
