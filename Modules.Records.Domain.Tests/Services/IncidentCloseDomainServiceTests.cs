using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.Services;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Domain.Tests.Services;

public sealed class IncidentCloseDomainServiceTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();

    private static Incident CreateIncident() =>
        new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = TestJurisdictionId,
            AgencyId = TestAgencyId,
            Details = new IncidentDetails
            {
                IncidentNum = "INC-001",
                LocalNum = ""
            }
        });

    private static Arrest MakeArrest(bool withName = true) =>
        new ArrestFactory().Create(
            TestJurisdictionId,
            TestAgencyId,
            withName ? Guid.NewGuid() : null,
            DateTime.UtcNow.AddDays(-1),
            "AR-001");

    private static Arrest MakeFinalizedArrest()
    {
        var arrest = MakeArrest();
        arrest.Finalize();
        return arrest;
    }

    private static Citation MakeIssuedCitation()
    {
        var citation = new Citation(TestJurisdictionId, TestAgencyId, "Speeding", DateTime.UtcNow.AddDays(-1), "CT-001");
        citation.Issue(new UserModificationContext(Guid.NewGuid()));
        return citation;
    }

    [Fact]
    public async Task ValidateCanCloseAsync_WhenForced_ReturnsWithoutChecking()
    {
        var incident = CreateIncident();
        var arrestRepo = new StubArrestRepository([MakeArrest(withName: false)]); // unfinalized
        var citationRepo = new StubCitationRepository([]);
        var rules = new StubJurisdictionRules(mustCloseArrests: false);

        var sut = new IncidentCloseDomainService(arrestRepo, citationRepo, rules);

        // Should not throw even though arrest is not finalized, because isForced=true
        await sut.ValidateCanCloseAsync(incident, isForced: true, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateCanCloseAsync_WithNoChildRecords_Succeeds()
    {
        var incident = CreateIncident();
        var sut = new IncidentCloseDomainService(
            new StubArrestRepository([]),
            new StubCitationRepository([]),
            new StubJurisdictionRules(false));

        await sut.ValidateCanCloseAsync(incident, isForced: false, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateCanCloseAsync_WithFinalizedArrests_AndIssuedCitations_Succeeds()
    {
        var incident = CreateIncident();
        var sut = new IncidentCloseDomainService(
            new StubArrestRepository([MakeFinalizedArrest()]),
            new StubCitationRepository([MakeIssuedCitation()]),
            new StubJurisdictionRules(false));

        await sut.ValidateCanCloseAsync(incident, isForced: false, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateCanCloseAsync_WithUnfinalizedArrest_ThrowsDomainException()
    {
        var incident = CreateIncident();
        var sut = new IncidentCloseDomainService(
            new StubArrestRepository([MakeArrest()]), // not finalized
            new StubCitationRepository([]),
            new StubJurisdictionRules(false));

        await Assert.ThrowsAsync<DomainException>(() =>
            sut.ValidateCanCloseAsync(incident, isForced: false, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateCanCloseAsync_WithUnissuedCitation_ThrowsDomainException()
    {
        var unissuedCitation = new Citation(TestJurisdictionId, TestAgencyId, "Speeding", DateTime.UtcNow.AddDays(-1), "CT-001");

        var incident = CreateIncident();
        var sut = new IncidentCloseDomainService(
            new StubArrestRepository([]),
            new StubCitationRepository([unissuedCitation]),
            new StubJurisdictionRules(false));

        await Assert.ThrowsAsync<DomainException>(() =>
            sut.ValidateCanCloseAsync(incident, isForced: false, CancellationToken.None));
    }

    [Fact]
    public void Constructor_WithNullArrestRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IncidentCloseDomainService(
                null!,
                new StubCitationRepository([]),
                new StubJurisdictionRules(false)));
    }

    [Fact]
    public void Constructor_WithNullCitationRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IncidentCloseDomainService(
                new StubArrestRepository([]),
                null!,
                new StubJurisdictionRules(false)));
    }

    [Fact]
    public void Constructor_WithNullJurisdictionRules_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IncidentCloseDomainService(
                new StubArrestRepository([]),
                new StubCitationRepository([]),
                null!));
    }

    #region Test Stubs

    private sealed class StubArrestRepository : IArrestRepository
    {
        private readonly IReadOnlyList<Arrest> _arrests;

        public StubArrestRepository(IReadOnlyList<Arrest> arrests)
        {
            _arrests = arrests;
        }

        public Task<IReadOnlyList<Arrest>> GetByIncidentIdAsync(Guid incidentId, CancellationToken cancellationToken)
            => Task.FromResult(_arrests);
    }

    private sealed class StubCitationRepository : ICitationRepository
    {
        private readonly IReadOnlyList<Citation> _citations;

        public StubCitationRepository(IReadOnlyList<Citation> citations)
        {
            _citations = citations;
        }

        public Task<IReadOnlyList<Citation>> GetByIncidentIdAsync(Guid incidentId, CancellationToken cancellationToken)
            => Task.FromResult(_citations);
    }

    private sealed class StubJurisdictionRules : IJurisdictionRulesService
    {
        private readonly bool _mustCloseArrests;

        public StubJurisdictionRules(bool mustCloseArrests)
        {
            _mustCloseArrests = mustCloseArrests;
        }

        public bool MustCloseAllArrests(Guid jurisdictionId) => _mustCloseArrests;
        public bool MustCloseAllCitations(Guid jurisdictionId) => false;
    }

    #endregion
}
