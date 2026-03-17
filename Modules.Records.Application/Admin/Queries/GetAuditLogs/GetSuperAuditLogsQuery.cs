using MediatR;

namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public sealed record GetSuperAuditLogsQuery(AuditLogSearchRequest Request) : IRequest<AuditLogQueryResult>;
