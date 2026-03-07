using MediatR;
using Modules.Records.Application.Configurations.Commands.DeleteAgencyConfiguration;
using Modules.Records.Application.Configurations.Commands.GenerateArrestNum;
using Modules.Records.Application.Configurations.Commands.GenerateCitationNum;
using Modules.Records.Application.Configurations.Commands.GenerateIncidentNum;
using Modules.Records.Application.Configurations.Commands.SetAgencyConfiguration;
using Modules.Records.Application.Configurations.Queries.GetAgencyConfiguration;
using Modules.Records.Application.Configurations.Queries.GetAgencyConfigurations;
using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public sealed class AgencyConfigurationService : IAgencyConfigurationService
{
    private readonly ISender _sender;

    public AgencyConfigurationService(ISender sender) => _sender = sender;

    public Task<IReadOnlyList<AgencyConfigurationDto>> GetAllAsync() =>
        _sender.Send(new GetAgencyConfigurationsQuery());

    public Task<AgencyConfigurationDto?> GetAsync(string key) =>
        _sender.Send(new GetAgencyConfigurationQuery(key));

    public async Task SetAsync(string key, string value) =>
        await _sender.Send(new SetAgencyConfigurationCommand(key, value));

    public async Task DeleteAsync(string key) =>
        await _sender.Send(new DeleteAgencyConfigurationCommand(key));

    public Task<string> GenerateIncidentNumAsync() =>
        _sender.Send(new GenerateIncidentNumCommand());

    public Task<string> GenerateArrestNumAsync() =>
        _sender.Send(new GenerateArrestNumCommand());

    public Task<string> GenerateCitationNumAsync() =>
        _sender.Send(new GenerateCitationNumCommand());
}
