using FluentValidation;

namespace Modules.Records.Application.Charges.Commands.UpdateCharge;

public sealed class UpdateChargeCommandValidator : AbstractValidator<UpdateChargeCommand>
{
    public UpdateChargeCommandValidator()
    {
        RuleFor(x => x.ChargeId).NotEmpty();
        RuleFor(x => x.OffenseName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UcrCategory).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NibrsGroup).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CrimeAgainst).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UcrCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ChargeLevel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StateClass).MaximumLength(50);
    }
}
