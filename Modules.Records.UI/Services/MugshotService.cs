using MediatR;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Mugshots.Commands.SetPrimaryMugshot;
using Modules.Records.Application.Mugshots.Commands.UnlinkMugshotFromOwner;
using Modules.Records.Application.Mugshots.Commands.UploadMugshot;
using Modules.Records.Application.Mugshots.Queries.GetMugshotsByOwner;
using Modules.Records.Domain.Common;

namespace Modules.Records.UI.Services;

public sealed class MugshotService : IMugshotService
{
    private readonly ISender _sender;

    public MugshotService(ISender sender)
    {
        _sender = sender;
    }

    public Task<IReadOnlyList<MugshotDto>> GetForNameAsync(Guid nameId) =>
        _sender.Send(new GetMugshotsByOwnerQuery(MugshotOwnerTypes.Name, nameId));

    public Task<IReadOnlyList<MugshotDto>> GetForArrestAsync(Guid arrestId) =>
        _sender.Send(new GetMugshotsByOwnerQuery(MugshotOwnerTypes.Arrest, arrestId));

    public Task<Guid> UploadForNameAsync(
        Guid nameId,
        string fileName,
        string contentType,
        byte[] content,
        bool makePrimary = false,
        DateTime? capturedAtUtc = null) =>
        _sender.Send(new UploadMugshotCommand(
            MugshotOwnerTypes.Name,
            nameId,
            fileName,
            contentType,
            content,
            makePrimary,
            capturedAtUtc));

    public Task<Guid> UploadForArrestAsync(
        Guid arrestId,
        string fileName,
        string contentType,
        byte[] content,
        bool makePrimary = false,
        DateTime? capturedAtUtc = null) =>
        _sender.Send(new UploadMugshotCommand(
            MugshotOwnerTypes.Arrest,
            arrestId,
            fileName,
            contentType,
            content,
            makePrimary,
            capturedAtUtc));

    public Task SetPrimaryForNameAsync(Guid nameId, Guid mugshotId) =>
        _sender.Send(new SetPrimaryMugshotCommand(mugshotId, MugshotOwnerTypes.Name, nameId));

    public Task SetPrimaryForArrestAsync(Guid arrestId, Guid mugshotId) =>
        _sender.Send(new SetPrimaryMugshotCommand(mugshotId, MugshotOwnerTypes.Arrest, arrestId));

    public Task RemoveFromNameAsync(Guid nameId, Guid mugshotId) =>
        _sender.Send(new UnlinkMugshotFromOwnerCommand(mugshotId, MugshotOwnerTypes.Name, nameId));

    public Task RemoveFromArrestAsync(Guid arrestId, Guid mugshotId) =>
        _sender.Send(new UnlinkMugshotFromOwnerCommand(mugshotId, MugshotOwnerTypes.Arrest, arrestId));
}
