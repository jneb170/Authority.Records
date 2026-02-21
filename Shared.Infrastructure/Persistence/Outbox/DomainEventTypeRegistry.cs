using Modules.Records.Domain.DomainEvents;

namespace Shared.Infrastructure.Persistence.Outbox;

/// <summary>
/// Provides a registry for mapping domain event type names to their corresponding <see cref="Type"/> objects.
/// This registry is used to deserialize domain events from the outbox by their assembly-qualified names.
/// </summary>
public static class DomainEventTypeRegistry
{
    /// <summary>
    /// Dictionary that maps assembly-qualified type names to their corresponding <see cref="Type"/> objects.
    /// </summary>
    private static readonly Dictionary<string, Type> _types;

    /// <summary>
    /// Static constructor that initializes the registry by discovering all domain event types
    /// in the assembly containing <see cref="IDomainEvent"/>.
    /// </summary>
    static DomainEventTypeRegistry()
    {
        var domainAssembly = typeof(IDomainEvent).Assembly;

        _types = domainAssembly
            .GetTypes()
            .Where(t =>
                typeof(IDomainEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                t.IsClass)
            .ToDictionary(t => t.AssemblyQualifiedName!);
    }

    /// <summary>
    /// Attempts to retrieve a domain event type by its assembly-qualified name.
    /// </summary>
    /// <param name="typeName">The assembly-qualified name of the domain event type.</param>
    /// <param name="type">When this method returns, contains the <see cref="Type"/> associated with the specified type name, if found; otherwise, null.</param>
    /// <returns><c>true</c> if the type was found in the registry; otherwise, <c>false</c>.</returns>
    public static bool TryGet(string typeName, out Type? type)
        => _types.TryGetValue(typeName, out type);
}
