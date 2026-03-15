using MediatR;

namespace StellarEmpires.Shared.Events;

public interface IDomainEvent : INotification
{
    string EventType { get; }
    Guid EntityId { get; }
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
