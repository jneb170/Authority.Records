using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.DomainInvariants.IncidentClose;

public sealed record IncidentCloseContext(
    Incident Incident,
    IReadOnlyList<Arrest> Arrests,
    IReadOnlyList<Citation> Citations);
