BEGIN TRANSACTION;

DROP TABLE IF EXISTS citation_highway_types;
DROP TABLE IF EXISTS highway_type_lut;
DROP TABLE IF EXISTS citation_area_types;
DROP TABLE IF EXISTS area_type_lut;
DROP TABLE IF EXISTS citation_incident_severities;
DROP TABLE IF EXISTS incident_severity_lut;
DROP TABLE IF EXISTS citation_collision_configurations;
DROP TABLE IF EXISTS collision_configuration_lut;
DROP TABLE IF EXISTS citation_environment_factors;
DROP TABLE IF EXISTS environment_factor_lut;
DROP TABLE IF EXISTS citation_parking_violations;
DROP TABLE IF EXISTS parking_violation_lut;
DROP TABLE IF EXISTS citation_movement_violations;
DROP TABLE IF EXISTS movement_violation_lut;
DROP TABLE IF EXISTS citation_violations;
DROP TABLE IF EXISTS speed_band_lut;
DROP TABLE IF EXISTS violation_group_lut;
DROP TABLE IF EXISTS court_appearances;
DROP TABLE IF EXISTS bonds;
DROP TABLE IF EXISTS citations;
DROP TABLE IF EXISTS violation_sources;
DROP TABLE IF EXISTS violation_source_type_lut;
DROP TABLE IF EXISTS vehicles;
DROP TABLE IF EXISTS officer_profiles;
DROP TABLE IF EXISTS driver_licenses;
DROP TABLE IF EXISTS persons;
DROP TABLE IF EXISTS courts;
DROP TABLE IF EXISTS addresses;

CREATE TABLE addresses (
  address_id INTEGER PRIMARY KEY,
  line1 TEXT NOT NULL,
  city TEXT,
  state_code TEXT
);

CREATE TABLE courts (
  court_id INTEGER PRIMARY KEY,
  court_name TEXT NOT NULL,
  jurisdiction_name TEXT,
  address_id INTEGER,
  FOREIGN KEY (address_id) REFERENCES addresses(address_id)
);

CREATE TABLE persons (
  person_id INTEGER PRIMARY KEY,
  last_name TEXT NOT NULL,
  first_name TEXT,
  middle_name TEXT,
  birth_date TEXT,
  age_years INTEGER,
  race_code TEXT,
  sex_code TEXT,
  height_text TEXT,
  weight_lbs INTEGER,
  occupation TEXT,
  ssn TEXT,
  address_id INTEGER,
  FOREIGN KEY (address_id) REFERENCES addresses(address_id)
);

CREATE TABLE driver_licenses (
  driver_license_id INTEGER PRIMARY KEY,
  person_id INTEGER NOT NULL,
  license_number TEXT NOT NULL,
  issuing_state_code TEXT,
  license_kind TEXT,
  UNIQUE (person_id, license_number, issuing_state_code),
  FOREIGN KEY (person_id) REFERENCES persons(person_id)
);

CREATE TABLE officer_profiles (
  officer_profile_id INTEGER PRIMARY KEY,
  person_id INTEGER NOT NULL,
  title TEXT,
  badge_or_identifier TEXT,
  unit_number TEXT,
  FOREIGN KEY (person_id) REFERENCES persons(person_id)
);

CREATE TABLE vehicles (
  vehicle_id INTEGER PRIMARY KEY,
  plate_number TEXT,
  plate_state_code TEXT,
  plate_year INTEGER,
  model_year INTEGER,
  make TEXT,
  style TEXT,
  color TEXT,
  is_commercial INTEGER NOT NULL DEFAULT 0 CHECK (is_commercial IN (0, 1)),
  carries_hazardous_material INTEGER NOT NULL DEFAULT 0 CHECK (carries_hazardous_material IN (0, 1))
);

CREATE TABLE violation_source_type_lut (
  violation_source_type_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE violation_sources (
  violation_source_id INTEGER PRIMARY KEY,
  violation_source_type_code TEXT NOT NULL,
  section_number TEXT,
  source_name TEXT,
  FOREIGN KEY (violation_source_type_code) REFERENCES violation_source_type_lut(violation_source_type_code)
);

CREATE TABLE violation_group_lut (
  violation_group_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE speed_band_lut (
  speed_band_code TEXT PRIMARY KEY,
  label TEXT NOT NULL,
  min_over_mph INTEGER,
  max_over_mph INTEGER
);

CREATE TABLE citations (
  citation_id INTEGER PRIMARY KEY,
  case_number TEXT,
  docket_number TEXT,
  page_number TEXT,
  court_id INTEGER,
  citation_date TEXT,
  citation_time TEXT,
  citation_time_period TEXT CHECK (citation_time_period IN ('AM', 'PM')),
  occurred_at_text TEXT,
  defendant_person_id INTEGER NOT NULL,
  vehicle_id INTEGER,
  violation_source_id INTEGER,
  narrative_other_violations TEXT,
  complainant_officer_profile_id INTEGER,
  affidavit_signed_date TEXT,
  complainant_signature_text TEXT,
  arrest_date_text TEXT,
  accepted_bond_notes TEXT,
  receipt_number TEXT,
  FOREIGN KEY (court_id) REFERENCES courts(court_id),
  FOREIGN KEY (defendant_person_id) REFERENCES persons(person_id),
  FOREIGN KEY (vehicle_id) REFERENCES vehicles(vehicle_id),
  FOREIGN KEY (violation_source_id) REFERENCES violation_sources(violation_source_id),
  FOREIGN KEY (complainant_officer_profile_id) REFERENCES officer_profiles(officer_profile_id)
);

CREATE TABLE bonds (
  bond_id INTEGER PRIMARY KEY,
  citation_id INTEGER NOT NULL,
  amount NUMERIC,
  bond_type TEXT,
  receipt_number TEXT,
  accepted_by_text TEXT,
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id)
);

CREATE TABLE court_appearances (
  appearance_id INTEGER PRIMARY KEY,
  citation_id INTEGER NOT NULL,
  appearance_date TEXT,
  appearance_time TEXT,
  appearance_time_period TEXT CHECK (appearance_time_period IN ('AM', 'PM')),
  court_address_id INTEGER,
  defendant_signature_text TEXT,
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (court_address_id) REFERENCES addresses(address_id)
);

CREATE TABLE citation_violations (
  citation_violation_id INTEGER PRIMARY KEY,
  citation_id INTEGER NOT NULL,
  violation_group_code TEXT NOT NULL,
  description TEXT NOT NULL,
  speed_mph INTEGER,
  zone_mph INTEGER,
  speed_band_code TEXT,
  detail_text TEXT,
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (violation_group_code) REFERENCES violation_group_lut(violation_group_code),
  FOREIGN KEY (speed_band_code) REFERENCES speed_band_lut(speed_band_code)
);

CREATE TABLE movement_violation_lut (
  movement_violation_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE citation_movement_violations (
  citation_id INTEGER NOT NULL,
  movement_violation_code TEXT NOT NULL,
  PRIMARY KEY (citation_id, movement_violation_code),
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (movement_violation_code) REFERENCES movement_violation_lut(movement_violation_code)
);

CREATE TABLE parking_violation_lut (
  parking_violation_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE citation_parking_violations (
  citation_id INTEGER NOT NULL,
  parking_violation_code TEXT NOT NULL,
  meter_number TEXT,
  area_text TEXT,
  detail_text TEXT,
  PRIMARY KEY (citation_id, parking_violation_code),
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (parking_violation_code) REFERENCES parking_violation_lut(parking_violation_code)
);

CREATE TABLE environment_factor_lut (
  environment_factor_code TEXT PRIMARY KEY,
  factor_group TEXT NOT NULL,
  label TEXT NOT NULL
);

CREATE TABLE citation_environment_factors (
  citation_id INTEGER NOT NULL,
  environment_factor_code TEXT NOT NULL,
  PRIMARY KEY (citation_id, environment_factor_code),
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (environment_factor_code) REFERENCES environment_factor_lut(environment_factor_code)
);

CREATE TABLE collision_configuration_lut (
  collision_configuration_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE citation_collision_configurations (
  citation_id INTEGER NOT NULL,
  collision_configuration_code TEXT NOT NULL,
  PRIMARY KEY (citation_id, collision_configuration_code),
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (collision_configuration_code) REFERENCES collision_configuration_lut(collision_configuration_code)
);

CREATE TABLE incident_severity_lut (
  incident_severity_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE citation_incident_severities (
  citation_id INTEGER NOT NULL,
  incident_severity_code TEXT NOT NULL,
  PRIMARY KEY (citation_id, incident_severity_code),
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (incident_severity_code) REFERENCES incident_severity_lut(incident_severity_code)
);

CREATE TABLE area_type_lut (
  area_type_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE citation_area_types (
  citation_id INTEGER NOT NULL,
  area_type_code TEXT NOT NULL,
  PRIMARY KEY (citation_id, area_type_code),
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (area_type_code) REFERENCES area_type_lut(area_type_code)
);

CREATE TABLE highway_type_lut (
  highway_type_code TEXT PRIMARY KEY,
  label TEXT NOT NULL
);

CREATE TABLE citation_highway_types (
  citation_id INTEGER NOT NULL,
  highway_type_code TEXT NOT NULL,
  PRIMARY KEY (citation_id, highway_type_code),
  FOREIGN KEY (citation_id) REFERENCES citations(citation_id),
  FOREIGN KEY (highway_type_code) REFERENCES highway_type_lut(highway_type_code)
);

INSERT INTO violation_source_type_lut (violation_source_type_code, label) VALUES
  ('state_statute', 'State Statute'),
  ('local_ordinance', 'Local Ordinance');

INSERT INTO violation_group_lut (violation_group_code, label) VALUES
  ('speed', 'Speed-related violation'),
  ('turn', 'Turning violation'),
  ('lane_position', 'Lane or roadway position violation'),
  ('parking', 'Parking violation'),
  ('other', 'Other violation');

INSERT INTO speed_band_lut (speed_band_code, label, min_over_mph, max_over_mph) VALUES
  ('over_5_to_10', '5-10 m.p.h. over limit', 5, 10),
  ('over_11_to_15', '11-15 m.p.h. over limit', 11, 15),
  ('over_15', 'Over 15 m.p.h. over limit', 16, NULL);

INSERT INTO movement_violation_lut (movement_violation_code, label) VALUES
  ('unable_to_stop_clear_distance_ahead', 'Unable to stop in assured clear distance ahead'),
  ('improper_left_turn', 'Improper LEFT TURN'),
  ('improper_right_turn', 'Improper RIGHT TURN'),
  ('no_signal', 'No Signal'),
  ('cut_corner', 'Cut corner'),
  ('from_wrong_lane', 'From wrong lane'),
  ('into_wrong_lane', 'Into wrong lane'),
  ('from_wrong_lane_no_intersection', 'From wrong lane not reached intersection'),
  ('past_middle_intersection', 'Past middle intersection'),
  ('middle_of_intersection', 'Middle of intersection'),
  ('wrong_place', 'Wrong place'),
  ('at_intersection', 'At intersection'),
  ('walk_speed', 'Walk speed'),
  ('faster', 'Faster'),
  ('cut_in', 'Cut in'),
  ('wrong_side_of_pavement', 'Wrong side of pavement'),
  ('divided_traffic', 'Divided traffic'),
  ('lane', 'Lane'),
  ('on_right', 'On right'),
  ('on_hill', 'On hill'),
  ('straddling', 'Straddling'),
  ('wrong_lane', 'Wrong lane'),
  ('on_curve', 'On curve');

INSERT INTO parking_violation_lut (parking_violation_code, label) VALUES
  ('other_parking_violation', 'Other parking violation'),
  ('overtime', 'Overtime'),
  ('area_parking', 'Area parking'),
  ('parking_prohibited', 'Parking prohibited'),
  ('double_parking', 'Double parking'),
  ('expired_meter', 'Expired meter');

INSERT INTO environment_factor_lut (environment_factor_code, factor_group, label) VALUES
  ('slippery_pavement_rain', 'surface_condition', 'Slippery pavement - Rain'),
  ('slippery_pavement_snow', 'surface_condition', 'Slippery pavement - Snow'),
  ('slippery_pavement_ice', 'surface_condition', 'Slippery pavement - Ice'),
  ('night', 'visibility_condition', 'Night'),
  ('fog', 'visibility_condition', 'Fog'),
  ('cross_traffic', 'traffic_presence', 'Cross traffic'),
  ('oncoming_traffic', 'traffic_presence', 'Oncoming traffic'),
  ('pedestrian_present', 'traffic_presence', 'Pedestrian'),
  ('same_direction_traffic', 'traffic_presence', 'Same direction'),
  ('caused_pedestrian_to_dodge', 'avoidance_event', 'Caused person to dodge - Pedestrian'),
  ('caused_driver_to_dodge', 'avoidance_event', 'Caused person to dodge - Driver'),
  ('near_miss', 'avoidance_event', 'Just missed accident');

INSERT INTO collision_configuration_lut (collision_configuration_code, label) VALUES
  ('pedestrian', 'Pedestrian'),
  ('pedestrian_vehicle', 'Pedestrian vehicle'),
  ('hit_fixed_object', 'Hit fixed object'),
  ('right_angle', 'Right angle'),
  ('head_on', 'Head on'),
  ('sideswipe', 'Sideswipe'),
  ('rear_end', 'Rear end'),
  ('ran_off_roadway', 'Ran off roadway'),
  ('intersection_related', 'Intersection');

INSERT INTO incident_severity_lut (incident_severity_code, label) VALUES
  ('property_damage', 'PD'),
  ('personal_injury', 'PI'),
  ('fatality', 'Fatal');

INSERT INTO area_type_lut (area_type_code, label) VALUES
  ('business', 'Business'),
  ('industrial', 'Industrial'),
  ('school', 'School'),
  ('residential', 'Residential'),
  ('rural', 'Rural');

INSERT INTO highway_type_lut (highway_type_code, label) VALUES
  ('two_lane_undivided', '2 lane undivided'),
  ('three_lane_undivided', '3 lane undivided'),
  ('four_lane_undivided', '4 lane undivided'),
  ('four_lane_divided', '4 lane divided');

COMMIT;
