using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Configurations.Queries.GetAgencyConfigurations;

/// <summary>Returns all active configuration entries for the current agency.</summary>
public sealed record GetAgencyConfigurationsQuery : IRequest<IReadOnlyList<AgencyConfigurationDto>>;
