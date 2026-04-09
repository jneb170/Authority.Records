BEGIN TRANSACTION;

-- Example data for the schema in texas_citation_schema.sql.
-- These values are fictional and are meant to demonstrate how the normalized
-- tables fit together, not to represent a realistic single incident.

INSERT INTO addresses (address_id, line1, city, state_code) VALUES
  (1, '100 Justice Plaza', 'Austin', 'TX'),
  (2, '742 Mockingbird Lane', 'Austin', 'TX');

INSERT INTO courts (court_id, court_name, jurisdiction_name, address_id) VALUES
  (1, 'Municipal Court', 'City of Austin', 1);

INSERT INTO persons (
  person_id,
  last_name,
  first_name,
  middle_name,
  birth_date,
  age_years,
  race_code,
  sex_code,
  height_text,
  weight_lbs,
  occupation,
  ssn,
  address_id
) VALUES
  (1, 'Driver', 'Jamie', 'Q', '1991-07-14', 34, 'W', 'F', '5-08', 145, 'Engineer', '111-22-3333', 2),
  (2, 'Officer', 'Taylor', 'R', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1);

INSERT INTO driver_licenses (
  driver_license_id,
  person_id,
  license_number,
  issuing_state_code,
  license_kind
) VALUES
  (1, 1, 'TXD12345678', 'TX', 'C');

INSERT INTO officer_profiles (
  officer_profile_id,
  person_id,
  title,
  badge_or_identifier,
  unit_number
) VALUES
  (1, 2, 'Officer', 'APD-4172', 'A-12');

INSERT INTO vehicles (
  vehicle_id,
  plate_number,
  plate_state_code,
  plate_year,
  model_year,
  make,
  style,
  color,
  is_commercial,
  carries_hazardous_material
) VALUES
  (1, 'TXN-4821', 'TX', 2026, 2022, 'Toyota', 'Sedan', 'Blue', 0, 0);

INSERT INTO violation_sources (
  violation_source_id,
  violation_source_type_code,
  section_number,
  source_name
) VALUES
  (1, 'state_statute', '545.351', 'Texas Transportation Code');

INSERT INTO citations (
  citation_id,
  case_number,
  docket_number,
  page_number,
  court_id,
  citation_date,
  citation_time,
  citation_time_period,
  occurred_at_text,
  defendant_person_id,
  vehicle_id,
  violation_source_id,
  narrative_other_violations,
  complainant_officer_profile_id,
  affidavit_signed_date,
  complainant_signature_text,
  arrest_date_text,
  accepted_bond_notes,
  receipt_number
) VALUES
  (
    1,
    'MC-2026-000123',
    'DKT-7781',
    '1',
    1,
    '2026-03-19',
    '09:42:00',
    'PM',
    'Congress Ave and 7th St',
    1,
    1,
    1,
    'Demonstration record with optional sections populated for schema examples.',
    1,
    '2026-03-19',
    'Officer Taylor R',
    '2026-03-19',
    'Cash bond accepted at booking desk',
    'R-100045'
  );

INSERT INTO bonds (
  bond_id,
  citation_id,
  amount,
  bond_type,
  receipt_number,
  accepted_by_text
) VALUES
  (1, 1, 250.00, 'cash', 'R-100045', 'Clerk Window 2');

INSERT INTO court_appearances (
  appearance_id,
  citation_id,
  appearance_date,
  appearance_time,
  appearance_time_period,
  court_address_id,
  defendant_signature_text
) VALUES
  (1, 1, '2026-04-21', '08:30:00', 'AM', 1, 'Jamie Q Driver');

INSERT INTO citation_violations (
  citation_violation_id,
  citation_id,
  violation_group_code,
  description,
  speed_mph,
  zone_mph,
  speed_band_code,
  detail_text
) VALUES
  (
    1,
    1,
    'speed',
    'Speeding (over limit)',
    46,
    30,
    'over_11_to_15',
    'Radar reading recorded by patrol unit.'
  ),
  (
    2,
    1,
    'other',
    'Unreasonable for conditions',
    46,
    30,
    NULL,
    'Wet pavement approaching intersection.'
  );

INSERT INTO citation_movement_violations (citation_id, movement_violation_code) VALUES
  (1, 'unable_to_stop_clear_distance_ahead'),
  (1, 'improper_left_turn'),
  (1, 'no_signal'),
  (1, 'from_wrong_lane');

INSERT INTO citation_parking_violations (
  citation_id,
  parking_violation_code,
  meter_number,
  area_text,
  detail_text
) VALUES
  (1, 'expired_meter', 'MTR-774', 'Downtown Lot B', NULL),
  (1, 'other_parking_violation', NULL, 'Loading Zone', 'Stopped in a signed loading zone after hours.');

INSERT INTO citation_environment_factors (citation_id, environment_factor_code) VALUES
  (1, 'slippery_pavement_rain'),
  (1, 'night'),
  (1, 'cross_traffic'),
  (1, 'caused_driver_to_dodge'),
  (1, 'near_miss');

INSERT INTO citation_collision_configurations (citation_id, collision_configuration_code) VALUES
  (1, 'rear_end'),
  (1, 'intersection_related');

INSERT INTO citation_incident_severities (citation_id, incident_severity_code) VALUES
  (1, 'property_damage');

INSERT INTO citation_area_types (citation_id, area_type_code) VALUES
  (1, 'business');

INSERT INTO citation_highway_types (citation_id, highway_type_code) VALUES
  (1, 'four_lane_undivided');

COMMIT;
