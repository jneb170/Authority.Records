using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Domain.Tests.ValueObjects;

public sealed class IncidentDetailsTests
{
    #region Validate Tests

    [Fact]
    public void Validate_WithValidData_ReturnsInstance()
    {
        var details = new IncidentDetails
        {
            IncidentNum = "INC-2026-001",
            LocalNum = "LOCAL-001",
            Description = "A routine incident",
            CFSNum = "CFS-123"
        };

        var result = details.Validate();

        Assert.Same(details, result);
    }

    [Fact]
    public void Validate_WithEmptyIncidentNum_ThrowsDomainException()
    {
        var details = new IncidentDetails
        {
            IncidentNum = "",
            LocalNum = ""
        };

        var ex = Assert.Throws<DomainException>(() => details.Validate());
        Assert.Equal("incident.incidentnum.empty", ex.Code);
    }

    [Fact]
    public void Validate_WithWhiteSpaceOnlyIncidentNum_ThrowsDomainException()
    {
        var details = new IncidentDetails
        {
            IncidentNum = "   ",
            LocalNum = ""
        };

        var ex = Assert.Throws<DomainException>(() => details.Validate());
        Assert.Equal("incident.incidentnum.empty", ex.Code);
    }

    [Fact]
    public void Validate_WithCFSNumExceeding30Chars_ThrowsDomainException()
    {
        var details = new IncidentDetails
        {
            IncidentNum = "INC-001",
            LocalNum = "",
            CFSNum = new string('X', 31)
        };

        var ex = Assert.Throws<DomainException>(() => details.Validate());
        Assert.Equal("incident.cfsnum.length", ex.Code);
    }

    [Fact]
    public void Validate_WithCFSNumExactly30Chars_DoesNotThrow()
    {
        var details = new IncidentDetails
        {
            IncidentNum = "INC-001",
            LocalNum = "",
            CFSNum = new string('X', 30)
        };

        var result = Record.Exception(() => details.Validate());
        Assert.Null(result);
    }

    [Fact]
    public void Validate_WithEmptyDescription_DoesNotThrow()
    {
        var details = new IncidentDetails
        {
            IncidentNum = "INC-001",
            LocalNum = "",
            Description = ""
        };

        var result = Record.Exception(() => details.Validate());
        Assert.Null(result);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var details1 = new IncidentDetails
        {
            IncidentNum = "INC-001",
            LocalNum = "L-001",
            Description = "Test",
            CFSNum = "CFS-001"
        };

        var details2 = new IncidentDetails
        {
            IncidentNum = "INC-001",
            LocalNum = "L-001",
            Description = "Test",
            CFSNum = "CFS-001"
        };

        Assert.Equal(details1, details2);
    }

    [Fact]
    public void RecordEquality_DifferentIncidentNum_AreNotEqual()
    {
        var details1 = new IncidentDetails { IncidentNum = "INC-001", LocalNum = "" };
        var details2 = new IncidentDetails { IncidentNum = "INC-002", LocalNum = "" };

        Assert.NotEqual(details1, details2);
    }

    #endregion
}
