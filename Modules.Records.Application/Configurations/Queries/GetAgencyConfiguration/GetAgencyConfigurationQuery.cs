using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Configurations.Queries.GetAgencyConfiguration;

/// <summary>Returns a single configuration entry by key for the current agency, or null if not set.</summary>
public sealed record GetAgencyConfigurationQuery(string Key) : IRequest<AgencyConfigurationDto?>;
