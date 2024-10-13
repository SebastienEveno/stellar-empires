using MediatR;

namespace StellarEmpires.Events;

public interface IDomainEvent : INotification
{
	Guid EntityId { get; }
	Guid Id { get; }
	DateTime OccurredOn { get; }
}
