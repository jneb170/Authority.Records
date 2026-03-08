using Modules.Records.Application.Admin.Commands.RebuildReadModels;

namespace Modules.Records.UI.Services;

public interface IAdminService
{
    Task<RebuildReadModelsResult> RebuildReadModelsAsync();
    Task SetRebuildScheduleAsync(string schedule);
    Task<string> GetRebuildScheduleAsync();
    Task<DateTime?> GetLastRebuildUtcAsync();
}
