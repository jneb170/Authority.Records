using MediatR;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Narratives.Commands.AcquireNarrativeLock;
using Modules.Records.Application.Narratives.Commands.CreateNarrative;
using Modules.Records.Application.Narratives.Commands.ReleaseNarrativeLock;
using Modules.Records.Application.Narratives.Commands.RenewNarrativeLock;
using Modules.Records.Application.Narratives.Commands.RestoreNarrative;
using Modules.Records.Application.Narratives.Commands.SoftDeleteNarrative;
using Modules.Records.Application.Narratives.Commands.UpdateNarrativeContent;
using Modules.Records.Application.Narratives.Queries.GetNarrativeById;
using Modules.Records.Application.Narratives.Queries.GetNarrativesByOwner;

namespace Modules.Records.UI.Services;

public sealed class NarrativeService : INarrativeService
{
    private readonly ISender _sender;

    public NarrativeService(ISender sender)
    {
        _sender = sender;
    }

    public Task<IReadOnlyList<NarrativeDto>> GetByOwnerAsync(string ownerType, Guid ownerId) =>
        _sender.Send(new GetNarrativesByOwnerQuery(ownerType, ownerId));

    public Task<NarrativeDto?> GetByIdAsync(Guid id) =>
        _sender.Send(new GetNarrativeByIdQuery(id));

    public Task<long> CreateAsync(string ownerType, Guid ownerId, string title, string content) =>
        _sender.Send(new CreateNarrativeCommand(ownerType, ownerId, title, content));

    public Task UpdateContentAsync(Guid id, string title, string content) =>
        _sender.Send(new UpdateNarrativeContentCommand(id, title, content));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireNarrativeLockCommand(id));

    public Task RenewLockAsync(Guid id) =>
        _sender.Send(new RenewNarrativeLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseNarrativeLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteNarrativeCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreNarrativeCommand(id));
}
