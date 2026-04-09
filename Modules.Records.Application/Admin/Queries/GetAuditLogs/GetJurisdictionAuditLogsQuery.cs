using MediatR;

namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public sealed record GetJurisdictionAuditLogsQuery(AuditLogSearchRequest Request) : IRequest<AuditLogQueryResult>;
