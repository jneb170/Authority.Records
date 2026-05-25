using System.Collections.ObjectModel;

namespace Modules.Records.Domain.Common.Violations;

/// <summary>One entry in the <see cref="ViolationFlagCatalog"/>: a flag key, its display label, and its section.</summary>
public sealed record ViolationFlagDefinition(ViolationFlagKey Key, string Label, ViolationFlagSection Section);

/// <summary>
/// The single source of truth describing every <see cref="ViolationFlagKey"/>: its human label and
/// the section it belongs to. Declaration order is the display order, so entry and print render the
/// checkboxes identically. Add a checkbox by adding a member to <see cref="ViolationFlagKey"/> and a
/// row here — both the entry form and the printed form pick it up automatically.
/// </summary>
public static class ViolationFlagCatalog
{
    private static readonly ReadOnlyCollection<ViolationFlagDefinition> _definitions = new(
    [
        // Offense grid
        new(ViolationFlagKey.UnreasonableForConditions, "Unreasonable for conditions", ViolationFlagSection.Offense),
        new(ViolationFlagKey.UnableToStop, "Unable to stop in assured clear distance ahead", ViolationFlagSection.Offense),
        new(ViolationFlagKey.ImproperLeftTurn, "Improper LEFT TURN", ViolationFlagSection.Offense),
        new(ViolationFlagKey.ImproperRightTurn, "Improper RIGHT TURN", ViolationFlagSection.Offense),
        new(ViolationFlagKey.ImproperPassingAndLaneUsage, "Improper PASSING AND LANE USAGE", ViolationFlagSection.Offense),
        new(ViolationFlagKey.NoSignal, "No Signal", ViolationFlagSection.Offense),
        new(ViolationFlagKey.SignalDeviceDisobeyed, "Signal device disobeyed", ViolationFlagSection.Offense),
        new(ViolationFlagKey.WrongPlace, "Wrong place", ViolationFlagSection.Offense),
        new(ViolationFlagKey.AtIntersection, "At intersection", ViolationFlagSection.Offense),
        new(ViolationFlagKey.Lane, "Lane", ViolationFlagSection.Offense),
        new(ViolationFlagKey.Straddling, "Straddling", ViolationFlagSection.Offense),
        new(ViolationFlagKey.CutCorner, "Cut corner", ViolationFlagSection.Offense),
        new(ViolationFlagKey.IntoWrongLane, "Into wrong lane", ViolationFlagSection.Offense),
        new(ViolationFlagKey.MiddleOfIntersection, "Middle of intersection", ViolationFlagSection.Offense),
        new(ViolationFlagKey.WalkSpeed, "Walk speed", ViolationFlagSection.Offense),
        new(ViolationFlagKey.CutIn, "Cut in", ViolationFlagSection.Offense),
        new(ViolationFlagKey.OnRight, "On right", ViolationFlagSection.Offense),
        new(ViolationFlagKey.WrongLane, "Wrong lane", ViolationFlagSection.Offense),
        new(ViolationFlagKey.FromWrongLane, "From wrong lane", ViolationFlagSection.Offense),
        new(ViolationFlagKey.WrongSideOfPavement, "Wrong side of pavement", ViolationFlagSection.Offense),
        new(ViolationFlagKey.Faster, "Faster", ViolationFlagSection.Offense),
        new(ViolationFlagKey.OnHill, "On hill", ViolationFlagSection.Offense),
        new(ViolationFlagKey.OnCurve, "On curve", ViolationFlagSection.Offense),

        // Contributors to last violation
        new(ViolationFlagKey.Rain, "Rain", ViolationFlagSection.Contributor),
        new(ViolationFlagKey.Snow, "Snow", ViolationFlagSection.Contributor),
        new(ViolationFlagKey.Ice, "Ice", ViolationFlagSection.Contributor),
        new(ViolationFlagKey.Night, "Night", ViolationFlagSection.Contributor),
        new(ViolationFlagKey.Fog, "Fog", ViolationFlagSection.Contributor),

        // Caused person to dodge
        new(ViolationFlagKey.DodgePedestrian, "Pedestrian", ViolationFlagSection.Dodge),
        new(ViolationFlagKey.DodgeDriver, "Driver", ViolationFlagSection.Dodge),
        new(ViolationFlagKey.JustMissedAccident, "Just missed accident", ViolationFlagSection.Dodge),

        // Type of collision
        new(ViolationFlagKey.CollisionPropertyDamage, "PD", ViolationFlagSection.Collision),
        new(ViolationFlagKey.CollisionPersonalInjury, "PI", ViolationFlagSection.Collision),
        new(ViolationFlagKey.CollisionFatal, "Fatal", ViolationFlagSection.Collision),
        new(ViolationFlagKey.CollisionVehicle, "Vehicle", ViolationFlagSection.Collision),
        new(ViolationFlagKey.HitFixedObject, "Hit fixed object", ViolationFlagSection.Collision),
        new(ViolationFlagKey.CollisionRightAngle, "Right angle", ViolationFlagSection.Collision),
        new(ViolationFlagKey.CollisionHeadOn, "Head on", ViolationFlagSection.Collision),
        new(ViolationFlagKey.CollisionSideswipe, "Sideswipe", ViolationFlagSection.Collision),
        new(ViolationFlagKey.CollisionRearEnd, "Rear end", ViolationFlagSection.Collision)
    ]);

    private static readonly IReadOnlyDictionary<ViolationFlagKey, ViolationFlagDefinition> _byKey =
        _definitions.ToDictionary(d => d.Key);

    /// <summary>All flag definitions in display order.</summary>
    public static IReadOnlyList<ViolationFlagDefinition> Definitions => _definitions;

    /// <summary>Flag definitions for a single section, in display order.</summary>
    public static IReadOnlyList<ViolationFlagDefinition> ForSection(ViolationFlagSection section)
        => _definitions.Where(d => d.Section == section).ToList();

    /// <summary>The display label for a flag key.</summary>
    public static string Label(ViolationFlagKey key) => _byKey[key].Label;

    /// <summary>The section a flag key belongs to.</summary>
    public static ViolationFlagSection Section(ViolationFlagKey key) => _byKey[key].Section;
}
