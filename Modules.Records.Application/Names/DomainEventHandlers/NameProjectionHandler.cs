using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Names.DomainEventHandlers;

public sealed class NameProjectionHandler :
    INotificationHandler<NameCreatedDomainEvent>,
    INotificationHandler<NameDetailsUpdatedDomainEvent>,
    INotificationHandler<NameSoftDeletedDomainEvent>,
    INotificationHandler<NameRestoredDomainEvent>,
    INotificationHandler<LockAcquiredDomainEvent<Name>>,
    INotificationHandler<LockReleasedDomainEvent<Name>>
{
    private readonly IApplicationDbContext _dbContext;

    public NameProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(NameCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.NameReadModels
            .AnyAsync(n => n.Id == notification.NameId, cancellationToken);
        if (exists) return;

        var name = await _dbContext.Names
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notification.NameId, cancellationToken);

        var readModel = NameReadModel.Create(
            id:                    notification.NameId,
            recordNumber:          name?.RecordNumber ?? 0,
            jurisdictionId:        notification.JurisdictionId,
            agencyId:              name?.AgencyId ?? Guid.Empty,
            nameType:              notification.NameType,
            lastOrBusinessName:    notification.LastOrBusinessName,
            firstName:             notification.FirstName,
            middleName:            notification.MiddleName,
            sexId:                 name?.SexId,
            raceId:                name?.RaceId,
            dateOfBirth:           name?.DateOfBirth,
            driversLicenseNumber:  name?.DriversLicenseNumber,
            driversLicenseStateId: name?.DriversLicenseStateId,
            heightInches:          name?.HeightInches,
            weightLbs:             name?.WeightLbs,
            hairColorId:           name?.HairColorId,
            eyeColorId:            name?.EyeColorId,
            suffixId:              name?.SuffixId,
            placeOfBirth:          name?.PlaceOfBirth,
            fbiNumber:             name?.FbiNumber,
            localNumber:           name?.LocalNumber,
            primaryPhone:          name?.PrimaryPhone,
            primaryPhoneExtension: name?.PrimaryPhoneExtension,
            workPhone:             name?.WorkPhone,
            workPhoneExtension:    name?.WorkPhoneExtension,
            otherPhone:            name?.OtherPhone,
            otherPhoneExtension:   name?.OtherPhoneExtension,
            socialSecurityNumber:  name?.SocialSecurityNumber,
            isCitizen:             name?.IsCitizen ?? false,
            deceasedDate:          name?.DeceasedDate,
            createdAtUtc:          notification.OccurredOnUtc,
            createdBy:             name?.CreatedBy ?? Guid.Empty);

        _dbContext.NameReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NameDetailsUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NameReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.NameId, cancellationToken);
        if (readModel is null) return;

        readModel.ApplyDetailsChanged(
            notification.NameType,
            notification.LastOrBusinessName,
            notification.FirstName,
            notification.MiddleName,
            notification.SexId,
            notification.RaceId,
            notification.DateOfBirth,
            notification.DriversLicenseNumber,
            notification.DriversLicenseStateId,
            notification.HeightInches,
            notification.WeightLbs,
            notification.HairColorId,
            notification.EyeColorId,
            notification.SuffixId,
            notification.PlaceOfBirth,
            notification.FbiNumber,
            notification.LocalNumber,
            notification.SocialSecurityNumber,
            notification.IsCitizen,
            notification.DeceasedDate,
            notification.PrimaryPhone,
            notification.PrimaryPhoneExtension,
            notification.WorkPhone,
            notification.WorkPhoneExtension,
            notification.OtherPhone,
            notification.OtherPhoneExtension);

        readModel.ApplyLocationChanged(notification.PrimaryLocationId, notification.SecondaryLocationId);
        readModel.ApplyModifiedAudit(notification.ModifiedBy, notification.OccurredOnUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NameSoftDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NameReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.NameId, cancellationToken);
        if (readModel is null) return;

        _dbContext.NameReadModels.Remove(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NameRestoredDomainEvent notification, CancellationToken cancellationToken)
    {
        // SoftDelete removed the read-model row (NameReadModel has no IsDeleted flag), so restore must
        // rebuild it from the aggregate or the name stays invisible in every list despite being live.
        var exists = await _dbContext.NameReadModels
            .AnyAsync(n => n.Id == notification.NameId, cancellationToken);
        if (exists) return;

        var name = await _dbContext.Names
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notification.NameId, cancellationToken);
        if (name is null) return;

        var readModel = NameReadModel.Create(
            id:                    name.Id,
            recordNumber:          name.RecordNumber,
            jurisdictionId:        name.JurisdictionId,
            agencyId:              name.AgencyId,
            nameType:              name.NameType,
            lastOrBusinessName:    name.LastOrBusinessName,
            firstName:             name.FirstName,
            middleName:            name.MiddleName,
            sexId:                 name.SexId,
            raceId:                name.RaceId,
            dateOfBirth:           name.DateOfBirth,
            driversLicenseNumber:  name.DriversLicenseNumber,
            driversLicenseStateId: name.DriversLicenseStateId,
            heightInches:          name.HeightInches,
            weightLbs:             name.WeightLbs,
            hairColorId:           name.HairColorId,
            eyeColorId:            name.EyeColorId,
            suffixId:              name.SuffixId,
            placeOfBirth:          name.PlaceOfBirth,
            fbiNumber:             name.FbiNumber,
            localNumber:           name.LocalNumber,
            primaryPhone:          name.PrimaryPhone,
            primaryPhoneExtension: name.PrimaryPhoneExtension,
            workPhone:             name.WorkPhone,
            workPhoneExtension:    name.WorkPhoneExtension,
            otherPhone:            name.OtherPhone,
            otherPhoneExtension:   name.OtherPhoneExtension,
            socialSecurityNumber:  name.SocialSecurityNumber,
            isCitizen:             name.IsCitizen,
            deceasedDate:          name.DeceasedDate,
            createdAtUtc:          name.CreatedAt,
            createdBy:             name.CreatedBy);

        readModel.ApplyLocationChanged(name.PrimaryLocationId, name.SecondaryLocationId);
        if (name.IsLocked && name.LockedByUserId is Guid lockedBy)
            readModel.ApplyLockAcquired(lockedBy);
        readModel.ApplyModifiedAudit(name.ModifiedBy, name.ModifiedAt, name.CreatedAt);

        _dbContext.NameReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockAcquiredDomainEvent<Name> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NameReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.AggregateId, cancellationToken);
        if (readModel is null) return;

        readModel.ApplyLockAcquired(notification.UserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockReleasedDomainEvent<Name> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NameReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.AggregateId, cancellationToken);
        if (readModel is null) return;

        readModel.ApplyLockReleased();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
