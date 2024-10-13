using StellarEmpires.Events;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Mocks;

public class MockDomainEvent : IDomainEvent
{
	public DateTime OccurredOn { get; private set; }

	public Guid Id => Guid.NewGuid();
	public Guid EntityId => Guid.NewGuid();

	public MockDomainEvent()
	{
		OccurredOn = DateTimeProvider.UtcNow;
	}
}
