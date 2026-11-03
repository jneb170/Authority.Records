using FluentValidation;
using Modules.Records.Application.Configurations.Commands.DeleteAgencyConfiguration;
using Modules.Records.Application.Configurations.Commands.SetAgencyConfiguration;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Configurations.Validators;

/// <summary>
/// Settings whose value is required for correct behavior. Deleting one silently fell back to a
/// default (for TimeZoneId, America/Chicago), which moves every incident time shown and printed
/// (issue #125). They can be changed but not deleted.
/// </summary>
public static class RequiredConfigurationKeys
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        ConfigurationKeys.TimeZoneId
    };
}

public sealed class SetAgencyConfigurationCommandValidator : AbstractValidator<SetAgencyConfigurationCommand>
{
    public SetAgencyConfigurationCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().WithMessage("Key is required.");
        RuleFor(x => x.Value).NotNull().WithMessage("Value is required.");

        RuleFor(x => x.Value)
            .Must(BeAKnownTimeZone)
            .When(x => x.Key == ConfigurationKeys.TimeZoneId)
            .WithMessage("TimeZoneId must be a valid time zone, for example America/Chicago.");

        RuleFor(x => x.Value)
            .Must(v => int.TryParse(v, out var seconds) && seconds is >= 60 and <= 86_400)
            .When(x => x.Key is ConfigurationKeys.LockTimeoutSeconds or ConfigurationKeys.NarrativeLockTimeoutSeconds)
            .WithMessage("Lock timeouts must be a whole number of seconds between 60 and 86400.");
    }

    private static bool BeAKnownTimeZone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return TimeZoneInfo.TryFindSystemTimeZoneById(value.Trim(), out _);
    }
}

public sealed class DeleteAgencyConfigurationCommandValidator : AbstractValidator<DeleteAgencyConfigurationCommand>
{
    public DeleteAgencyConfigurationCommandValidator()
    {
        RuleFor(x => x.Key)
            .Must(key => !RequiredConfigurationKeys.All.Contains(key))
            .WithMessage(x => $"{x.Key} is a required setting and can't be deleted. Change its value instead.");
    }
}
