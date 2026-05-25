namespace Modules.Records.Domain.Common.Violations;

/// <summary>
/// The catalog of structured violation checkbox flags that can be set on a citation. Each value
/// is a stable, jurisdiction-neutral key — it is persisted by <em>name</em> (see
/// <c>CitationViolationFlag</c>), so the value order here may be rearranged and members renamed in
/// code is the only thing that breaks identity. To add a checkbox: add a member here and a matching
/// entry in <see cref="ViolationFlagCatalog"/> — no migration is required.
/// </summary>
/// <remarks>
/// These keys replace the legacy free-text substring inference on the printed Texas form, which
/// produced false marks (e.g. "Ice" inside "device"). They describe driving facts that are common
/// across jurisdictions; which subset a given state form renders is a presentation concern.
/// </remarks>
public enum ViolationFlagKey
{
    // --- Offense grid ---
    UnreasonableForConditions,
    UnableToStop,
    ImproperLeftTurn,
    ImproperRightTurn,
    ImproperPassingAndLaneUsage,
    NoSignal,
    SignalDeviceDisobeyed,
    WrongPlace,
    AtIntersection,
    Lane,
    Straddling,
    CutCorner,
    IntoWrongLane,
    MiddleOfIntersection,
    WalkSpeed,
    CutIn,
    OnRight,
    WrongLane,
    FromWrongLane,
    WrongSideOfPavement,
    Faster,
    OnHill,
    OnCurve,

    // --- Contributors to last violation ---
    Rain,
    Snow,
    Ice,
    Night,
    Fog,

    // --- Caused person to dodge ---
    DodgePedestrian,
    DodgeDriver,
    JustMissedAccident,

    // --- Type of collision ---
    CollisionPropertyDamage,
    CollisionPersonalInjury,
    CollisionFatal,
    CollisionVehicle,
    HitFixedObject,
    CollisionRightAngle,
    CollisionHeadOn,
    CollisionSideswipe,
    CollisionRearEnd
}
