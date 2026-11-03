using MediatR;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Application.Arrests.Commands.GenerateTestArrests;
using Modules.Records.Application.Common.Behaviors;
using Modules.Records.Application.Configurations.Commands.DeleteAgencyConfiguration;
using Modules.Records.Application.Configurations.Commands.SetAgencyConfiguration;
using Modules.Records.Application.Configurations.Validators;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Tests.Authorization;

/// <summary>Issue #125: admin-only and demo-only requests are enforced in the pipeline.</summary>
public sealed class AuthorizationBehaviorTests
{
    [Theory]
    [InlineData(RoleNames.Supervisor)]
    [InlineData(RoleNames.Officer)]
    public async Task Supervisor_and_officer_cannot_change_settings_or_rebuild(string role)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Run(new SetAgencyConfigurationCommand(ConfigurationKeys.TimeZoneId, "UTC"), role));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Run(new DeleteAgencyConfigurationCommand(ConfigurationKeys.LockTimeoutSeconds), role));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Run(new RebuildReadModelsCommand(), role));
    }

    [Fact]
    public async Task Admin_can_change_settings_and_rebuild()
    {
        Assert.True(await Run(new SetAgencyConfigurationCommand(ConfigurationKeys.TimeZoneId, "UTC"), RoleNames.Admin));
        Assert.True(await Run(new RebuildReadModelsCommand(), RoleNames.Admin));
    }

    [Fact]
    public async Task Test_data_is_refused_outside_demo_mode_even_for_admin()
    {
        var command = new GenerateTestArrestsCommand(1, DateTime.UtcNow, DateTime.UtcNow,
            TestDataRecordLinkStrategy.CreateNew, TestDataRecordLinkStrategy.CreateNew);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Run(command, RoleNames.Admin, demo: false));
        Assert.True(await Run(command, RoleNames.Supervisor, demo: true));
    }

    [Fact]
    public async Task Requests_without_attributes_are_not_affected()
    {
        var command = new CreateNameCommand("Person", "Doe", null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, false, null);
        Assert.True(await Run(command, role: "none", demo: false));
    }

    [Fact]
    public void TimeZoneId_cannot_be_deleted_and_must_be_valid()
    {
        Assert.False(new DeleteAgencyConfigurationCommandValidator()
            .Validate(new DeleteAgencyConfigurationCommand(ConfigurationKeys.TimeZoneId)).IsValid);
        Assert.True(new DeleteAgencyConfigurationCommandValidator()
            .Validate(new DeleteAgencyConfigurationCommand(ConfigurationKeys.MapStartCoordinates)).IsValid);

        var set = new SetAgencyConfigurationCommandValidator();
        Assert.True(set.Validate(new SetAgencyConfigurationCommand(ConfigurationKeys.TimeZoneId, "America/Chicago")).IsValid);
        Assert.False(set.Validate(new SetAgencyConfigurationCommand(ConfigurationKeys.TimeZoneId, "Mars/Olympus")).IsValid);
        Assert.False(set.Validate(new SetAgencyConfigurationCommand(ConfigurationKeys.LockTimeoutSeconds, "5")).IsValid);
        Assert.True(set.Validate(new SetAgencyConfigurationCommand(ConfigurationKeys.LockTimeoutSeconds, "600")).IsValid);
    }

    private static async Task<bool> Run(IRequest request, string role, bool demo = true)
    {
        var reached = false;
        var behaviorType = typeof(AuthorizationBehavior<,>).MakeGenericType(request.GetType(), typeof(Unit));
        dynamic behavior = Activator.CreateInstance(behaviorType, new UserInRole(role), Options.Create(new DemoModeOptions { Enabled = demo }))!;
        RequestHandlerDelegate<Unit> next = _ => { reached = true; return Task.FromResult(Unit.Value); };
        await behavior.Handle((dynamic)request, next, CancellationToken.None);
        return reached;
    }

    /// <summary>Runs the behavior; returns true when it let the request through to the handler.</summary>
    private static async Task<bool> Run<TResponse>(IRequest<TResponse> request, string role, bool demo = true)
    {
        var reached = false;
        var behaviorType = typeof(AuthorizationBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic behavior = Activator.CreateInstance(behaviorType, new UserInRole(role), Options.Create(new DemoModeOptions { Enabled = demo }))!;
        RequestHandlerDelegate<TResponse> next = _ => { reached = true; return Task.FromResult(default(TResponse)!); };
        await behavior.Handle((dynamic)request, next, CancellationToken.None);
        return reached;
    }

    private sealed class UserInRole(string role) : ICurrentUserContext
    {
        public bool IsInRole(string roleName) => roleName == role;
        public bool IsDemoUser => false;
    }
}
