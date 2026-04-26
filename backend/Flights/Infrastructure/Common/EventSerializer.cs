using System.Text.Json;
using Flights.Domain.Interfaces;
using Flights.Domain.Models;

namespace Flights.Infrastructure.Common;

public static class EventSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static OutboxMessage Serialize(IDomainEvent @event)
    {
        var type = @event.GetType().FullName!;
        var data = JsonSerializer.Serialize(@event, @event.GetType(), _options);
        
        return OutboxMessage.Create(type, data);
    }

    public static IDomainEvent Deserialize(string type, string data)
    {
        var eventType = Type.GetType(type);
        if (eventType == null)
            throw new InvalidOperationException($"Event type '{type}' not found. Assembly not loaded or renamed.");

        return (IDomainEvent)JsonSerializer.Deserialize(data, eventType, _options)!;
    }
}