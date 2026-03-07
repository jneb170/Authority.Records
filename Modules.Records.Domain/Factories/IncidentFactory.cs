using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Factories;

public sealed class IncidentFactory
{
    public Incident Create(CreateIncidentRequest request)
        => new Incident(request);
}
