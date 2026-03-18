using FluentValidation;

namespace Modules.Records.Application.Charges.Commands.CreateCharge;

public sealed class CreateChargeCommandValidator : AbstractValidator<CreateChargeCommand>
{
    public CreateChargeCommandValidator()
    {
        RuleFor(x => x.OffenseName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UcrCategory).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NibrsGroup).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CrimeAgainst).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UcrCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ChargeLevel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StateClass).MaximumLength(50);
    }
}
