using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Queries.SearchNames;

public sealed class SearchNamesHandler : IRequestHandler<SearchNamesQuery, IReadOnlyList<NameDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SearchNamesHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<NameDto>> Handle(
        SearchNamesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.NameReadModels
            .AsNoTracking()
            .Where(n => n.JurisdictionId == _tenantProvider.GetJurisdictionId());

        if (!string.IsNullOrWhiteSpace(request.NameType))
            query = query.Where(n => n.NameType == request.NameType);

        if (!string.IsNullOrWhiteSpace(request.NameContains))
        {
            var term = request.NameContains.ToLower();
            query = query.Where(n =>
                n.LastOrBusinessName.ToLower().Contains(term) ||
                (n.FirstName != null && n.FirstName.ToLower().Contains(term)) ||
                (n.MiddleName != null && n.MiddleName.ToLower().Contains(term)));
        }

        if (request.SexId.HasValue)
            query = query.Where(n => n.SexId == request.SexId);

        if (request.RaceId.HasValue)
            query = query.Where(n => n.RaceId == request.RaceId);

        if (request.DateOfBirthFrom.HasValue)
            query = query.Where(n => n.DateOfBirth >= request.DateOfBirthFrom);

        if (request.DateOfBirthTo.HasValue)
            query = query.Where(n => n.DateOfBirth <= request.DateOfBirthTo);

        if (request.HeightInchesMin.HasValue)
            query = query.Where(n => n.HeightInches >= request.HeightInchesMin);

        if (request.HeightInchesMax.HasValue)
            query = query.Where(n => n.HeightInches <= request.HeightInchesMax);

        if (request.WeightLbsMin.HasValue)
            query = query.Where(n => n.WeightLbs >= request.WeightLbsMin);

        if (request.WeightLbsMax.HasValue)
            query = query.Where(n => n.WeightLbs <= request.WeightLbsMax);

        if (request.HairColorId.HasValue)
            query = query.Where(n => n.HairColorId == request.HairColorId);

        if (request.EyeColorId.HasValue)
            query = query.Where(n => n.EyeColorId == request.EyeColorId);

        if (!string.IsNullOrWhiteSpace(request.DriversLicenseNumber))
            query = query.Where(n => n.DriversLicenseNumber == request.DriversLicenseNumber);

        if (request.DriversLicenseStateId.HasValue)
            query = query.Where(n => n.DriversLicenseStateId == request.DriversLicenseStateId);

        var results = await query
            .OrderBy(n => n.LastOrBusinessName)
            .ThenBy(n => n.FirstName)
            .ToListAsync(cancellationToken);

        return results.Select(r => r.ToDto()).ToList();
    }
}
