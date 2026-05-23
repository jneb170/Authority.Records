using MediatR;
using System;

namespace Modules.Records.Application.Citations.Commands.RenewCitationLock
{
    public sealed record RenewCitationLockCommand(Guid CitationId) : IRequest;
}
