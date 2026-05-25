using Modules.Records.Domain.Common.Violations;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class CitationViolationFlagTests
{
    private static readonly Guid JurisdictionId = Guid.NewGuid();
    private static readonly Guid AgencyId = Guid.NewGuid();
    private static readonly Guid CitationId = Guid.NewGuid();

    [Fact]
    public void Constructor_DefaultsToManualSource_WithNoSourceCharge()
    {
        var flag = new CitationViolationFlag(JurisdictionId, AgencyId, CitationId, ViolationFlagKey.NoSignal);

        Assert.NotEqual(Guid.Empty, flag.Id);
        Assert.Equal(CitationId, flag.CitationId);
        Assert.Equal(ViolationFlagKey.NoSignal, flag.Key);
        Assert.Equal(ViolationFlagSource.Manual, flag.Source);
        Assert.Null(flag.SourceChargeLinkId);
    }

    [Fact]
    public void Constructor_RecordsChargeProvenance_WhenDerived()
    {
        var chargeLinkId = Guid.NewGuid();

        var flag = new CitationViolationFlag(
            JurisdictionId, AgencyId, CitationId, ViolationFlagKey.ImproperPassingAndLaneUsage,
            ViolationFlagSource.Charge, chargeLinkId);

        Assert.Equal(ViolationFlagSource.Charge, flag.Source);
        Assert.Equal(chargeLinkId, flag.SourceChargeLinkId);
    }
}
