using Modules.Records.Application.Relationships.Queries.GetRecordRelationships;

namespace Modules.Records.UI.Services;

public interface IRelationshipService
{
    Task<RecordRelationshipsDto?> GetAsync(string recordType, long recordNumber);
}
