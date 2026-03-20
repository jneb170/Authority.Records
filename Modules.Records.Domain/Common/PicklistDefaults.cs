namespace Modules.Records.Domain.Common;

/// <summary>
/// System-provided default item labels seeded when an agency first uses a picklist type.
/// The Value is derived from the label (stripped of spaces and special chars) for stability.
/// </summary>
public static class PicklistDefaults
{
    /// <summary>
    /// Returns default (value, label) pairs for a given picklist type.
    /// Returns an empty list for types with no system defaults (e.g. Court — fully agency-defined).
    /// </summary>
    public static IReadOnlyList<(string Value, string Label)> For(string picklistType) =>
        picklistType switch
        {
            PicklistTypes.ArrestType =>
            [
                ("OnView",        "On View"),
                ("Warrant",       "Warrant"),
                ("Summons",       "Summons"),
                ("Custodial",     "Custodial"),
                ("ProbableCause", "Probable Cause"),
                ("OtherAgency",   "Other Agency"),
            ],
            PicklistTypes.Direction =>
            [
                ("N",  "N"),
                ("NE", "NE"),
                ("E",  "E"),
                ("SE", "SE"),
                ("S",  "S"),
                ("SW", "SW"),
                ("W",  "W"),
                ("NW", "NW"),
            ],
            PicklistTypes.StreetType =>
            [
                ("St",   "St"),
                ("Ave",  "Ave"),
                ("Blvd", "Blvd"),
                ("Dr",   "Dr"),
                ("Ct",   "Ct"),
                ("Ln",   "Ln"),
                ("Pl",   "Pl"),
                ("Rd",   "Rd"),
                ("Way",  "Way"),
                ("Hwy",  "Hwy"),
                ("Pkwy", "Pkwy"),
            ],
            PicklistTypes.Country =>
            [
                ("US", "United States"),
                ("CA", "Canada"),
                ("MX", "Mexico"),
            ],
            PicklistTypes.ViolationSourceType =>
            [
                ("state_statute", "State Statute"),
                ("local_ordinance", "Local Ordinance")
            ],
            PicklistTypes.ViolationGroup =>
            [
                ("speed", "Speed-related violation"),
                ("turn", "Turning violation"),
                ("lane_position", "Lane or roadway position violation"),
                ("parking", "Parking violation"),
                ("other", "Other violation")
            ],
            PicklistTypes.SpeedBand =>
            [
                ("over_5_to_10", "5-10 m.p.h. over limit"),
                ("over_11_to_15", "11-15 m.p.h. over limit"),
                ("over_15", "Over 15 m.p.h. over limit")
            ],
            PicklistTypes.MovementViolation =>
            [
                ("unable_to_stop_clear_distance_ahead", "Unable to stop in assured clear distance ahead"),
                ("improper_left_turn", "Improper LEFT TURN"),
                ("improper_right_turn", "Improper RIGHT TURN"),
                ("no_signal", "No Signal"),
                ("cut_corner", "Cut corner"),
                ("from_wrong_lane", "From wrong lane"),
                ("into_wrong_lane", "Into wrong lane"),
                ("from_wrong_lane_no_intersection", "From wrong lane not reached intersection"),
                ("past_middle_intersection", "Past middle intersection"),
                ("middle_of_intersection", "Middle of intersection"),
                ("wrong_place", "Wrong place"),
                ("at_intersection", "At intersection"),
                ("walk_speed", "Walk speed"),
                ("faster", "Faster"),
                ("cut_in", "Cut in"),
                ("wrong_side_of_pavement", "Wrong side of pavement"),
                ("divided_traffic", "Divided traffic"),
                ("lane", "Lane"),
                ("on_right", "On right"),
                ("on_hill", "On hill"),
                ("straddling", "Straddling"),
                ("wrong_lane", "Wrong lane"),
                ("on_curve", "On curve")
            ],
            PicklistTypes.ParkingViolation =>
            [
                ("other_parking_violation", "Other parking violation"),
                ("overtime", "Overtime"),
                ("area_parking", "Area parking"),
                ("parking_prohibited", "Parking prohibited"),
                ("double_parking", "Double parking"),
                ("expired_meter", "Expired meter")
            ],
            PicklistTypes.EnvironmentFactor =>
            [
                ("slippery_pavement_rain", "Slippery pavement - Rain"),
                ("slippery_pavement_snow", "Slippery pavement - Snow"),
                ("slippery_pavement_ice", "Slippery pavement - Ice"),
                ("night", "Night"),
                ("fog", "Fog"),
                ("cross_traffic", "Cross traffic"),
                ("oncoming_traffic", "Oncoming traffic"),
                ("pedestrian_present", "Pedestrian"),
                ("same_direction_traffic", "Same direction"),
                ("caused_pedestrian_to_dodge", "Caused person to dodge - Pedestrian"),
                ("caused_driver_to_dodge", "Caused person to dodge - Driver"),
                ("near_miss", "Just missed accident")
            ],
            PicklistTypes.CollisionConfiguration =>
            [
                ("pedestrian", "Pedestrian"),
                ("pedestrian_vehicle", "Pedestrian vehicle"),
                ("hit_fixed_object", "Hit fixed object"),
                ("right_angle", "Right angle"),
                ("head_on", "Head on"),
                ("sideswipe", "Sideswipe"),
                ("rear_end", "Rear end"),
                ("ran_off_roadway", "Ran off roadway"),
                ("intersection_related", "Intersection")
            ],
            PicklistTypes.IncidentSeverity =>
            [
                ("property_damage", "PD"),
                ("personal_injury", "PI"),
                ("fatality", "Fatal")
            ],
            PicklistTypes.AreaType =>
            [
                ("business", "Business"),
                ("industrial", "Industrial"),
                ("school", "School"),
                ("residential", "Residential"),
                ("rural", "Rural")
            ],
            PicklistTypes.HighwayType =>
            [
                ("two_lane_undivided", "2 lane undivided"),
                ("three_lane_undivided", "3 lane undivided"),
                ("four_lane_undivided", "4 lane undivided"),
                ("four_lane_divided", "4 lane divided")
            ],
            _ => []
        };
}
