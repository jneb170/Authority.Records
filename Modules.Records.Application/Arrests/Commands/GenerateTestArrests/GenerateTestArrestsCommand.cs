using MediatR;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Arrests.Commands.GenerateTestArrests;

[DemoOnly]
[RequiresRole(RoleNames.Admin, RoleNames.Supervisor)]
public sealed record GenerateTestArrestsCommand(
    int Count,
    DateTime ArrestedFrom,
    DateTime ArrestedTo,
    TestDataRecordLinkStrategy NameStrategy,
    TestDataRecordLinkStrategy LocationStrategy,
    int NameMaxUses = 1,
    int LocationMaxUses = 1,
    string? LocationKeyword = null,
    string? LocationApiKey = null) : IRequest<GenerateTestArrestsResult>;
