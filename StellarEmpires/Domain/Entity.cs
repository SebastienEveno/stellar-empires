using StellarEmpires.Events;

namespace StellarEmpires.Domain;

public abstract class Entity : IEquatable<Entity>
{
	private readonly List<IDomainEvent> _domainEvents;

	public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	public Guid Id { get; private set; }

	protected Entity()
	{
		Id = Guid.NewGuid();
		_domainEvents = new List<IDomainEvent>();
	}

	protected Entity(Guid id)
	{
		Id = id;
		_domainEvents = new List<IDomainEvent>();
	}

	protected void AddDomainEvent(IDomainEvent domainEvent)
	{
		_domainEvents.Add(domainEvent);
	}

	public void ClearDomainEvents()
	{
		_domainEvents?.Clear();
	}

	public void LoadFromHistory(IEnumerable<IDomainEvent> history)
	{
		foreach (var domainEvent in history)
		{
			Apply(domainEvent);
		}
	}

	public Entity Clone()
	{
		return (Entity)MemberwiseClone();
	}

	public abstract void Apply(IDomainEvent domainEvent);

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public bool Equals(Entity? other)
	{
		if (other == null)
		{
			return false;
		}

		return Id == other.Id;
	}
}
