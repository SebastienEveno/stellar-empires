using StellarEmpires.Events;

namespace StellarEmpires.Infrastructure;

public interface IEventStore
{
	Task SaveEventAsync<TEntity>(IDomainEvent @event);
	Task<IEnumerable<IDomainEvent>> GetEventsAsync<TEntity>(Guid entityId);
}
