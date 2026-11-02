using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Arrests.Commands.CloseArrest;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Arrests.Commands.FinalizeArrest;
using Modules.Records.Application.Arrests.Commands.OpenArrest;
using Modules.Records.Application.Arrests.Commands.SaveArrestPage;
using Modules.Records.Application.Arrests.Commands.SoftDeleteArrest;
using Modules.Records.Application.Charges.Commands.SeedCharges;
using Modules.Records.Application.Citations.Commands.CreateCitation;
using Modules.Records.Application.Citations.Commands.IssueCitation;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;
using Modules.Records.Application.Citations.Commands.SoftDeleteCitation;
using Modules.Records.Application.Incidents.Commands.CloseIncident;
using Modules.Records.Application.Incidents.Commands.CreateIncident;
using Modules.Records.Application.Incidents.Commands.OpenIncident;
using Modules.Records.Application.Incidents.Commands.SaveIncidentPage;
using Modules.Records.Application.Incidents.Commands.SoftDeleteIncident;
using Modules.Records.Application.Locations.Commands.CreateLocation;
using Modules.Records.Application.Names.Commands.AcquireNameLock;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.Names.Commands.ReleaseNameLock;
using Modules.Records.Application.Names.Commands.UpdateNameDetails;
using Modules.Records.Application.Narratives.Commands.CreateNarrative;
using Modules.Records.Application.Picklists.Commands.SeedDefaultPicklistItems;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.ValueObjects;
using Modules.Records.UI.Helpers;
using Modules.Records.UI.Services;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Demo;

/// <summary>
/// Curated sample data for the public demo agency: a small, coherent set of
/// people, places, incidents, arrests and citations that reference each other
/// the way real records do (a suspect's arrest is linked to the incident, a
/// repeat subject shows up across several records, and so on).
///
/// Everything is written through the application's own MediatR commands while
/// signed in as the demo admin, so the data goes through the same validation,
/// domain rules, audit stamps and read-model projections as data a user enters.
///
/// Runs once per demo jurisdiction: it is skipped as soon as any Name exists
/// there. Each record is created independently, so one failure is logged and
/// the rest still seed.
/// </summary>
internal static class DemoDataset
{
    private const string City = "Riverbend";
    private const string StateCode = "TX";
    private const string Zip = "78640";

    /// <summary>Record numbers the previous, minimal seeder used. Replaced by this dataset.</summary>
    private static readonly string[] LegacyIncidentNums = ["DEMO-2026-001", "DEMO-2026-002", "DEMO-2026-003", "DEMO-2026-004"];
    private static readonly string[] LegacyArrestNums = ["DEMO-AR-2026-001", "DEMO-AR-2026-002"];
    private static readonly string[] LegacyCitationNums = ["DEMO-CT-2026-001", "DEMO-CT-2026-002"];

    public static async Task SeedAsync(
        IServiceProvider sp,
        ApplicationUser actor,
        Guid jurisdictionId,
        Guid agencyId,
        ILogger logger)
    {
        // Act as the demo admin for the rest of this scope. The tenant provider,
        // current-user context and modification context all read the scope's
        // authentication state, exactly as they do inside a Blazor circuit.
        var principalFactory = sp.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var principal = await principalFactory.CreateAsync(actor);
        if (sp.GetRequiredService<AuthenticationStateProvider>() is not IHostEnvironmentAuthenticationStateProvider hostAuth)
            throw new InvalidOperationException("The authentication state provider does not accept a host-set user.");
        hostAuth.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));

        var db = sp.GetRequiredService<AppDbContext>();
        if (await db.Names.IgnoreQueryFilters().AnyAsync(n => n.JurisdictionId == jurisdictionId))
            return;

        var seeder = new Seeder(sp, db, logger);
        await seeder.RunAsync();

        logger.LogInformation(
            "Demo dataset seeded for agency {AgencyId}: {Locations} locations, {Names} names, {Incidents} incidents, {Arrests} arrests, {Citations} citations ({Failures} step(s) failed).",
            agencyId, seeder.Locations.Count, seeder.Names.Count, seeder.Incidents.Count,
            seeder.Arrests.Count, seeder.Citations.Count, seeder.Failures);
    }

    // ------------------------------------------------------------------
    // Data
    // ------------------------------------------------------------------

    private sealed record PlaceSeed(
        string Key, string? Number, string Street, string? StreetType, string? PlaceName,
        double Lat, double Lng, string? Apt = null, string? PreDirection = null);

    private sealed record PersonSeed(
        string Key, string Last, string First, string? Middle, string Sex, string Race,
        DateTime Dob, int Height, int Weight, string Hair, string Eyes, string? Dl,
        string? HomeKey, string? Phone);

    private sealed record IncidentSeed(
        string Key, string Num, string Cfs, string Summary, string PlaceKey, TimeSpan Ago,
        string[] Charges, string Narrative, RecordStatus Status);

    private sealed record ArrestSeed(
        string Key, string PersonKey, string IncidentKey, string PlaceKey, string ArrestType,
        TimeSpan Ago, string[] Charges, string? Narrative, bool Close);

    private sealed record CitationSeed(
        string Key, string PersonKey, string? IncidentKey, string PlaceKey, TimeSpan Ago,
        string Description, string[] Charges);

    private static TimeSpan Days(double d) => TimeSpan.FromDays(d);
    private static TimeSpan Hours(double h) => TimeSpan.FromHours(h);

    private static readonly PlaceSeed[] Places =
    [
        new("pharmacy",  "4521", "Maple",      "Ave",  "Maple Avenue Pharmacy",     30.0712, -97.9318),
        new("5th-main",  null,   "5th St & Main", null, null,                       30.0668, -97.9405),
        new("library",   "200",  "Library",    "Ln",   "Riverbend Public Library",  30.0651, -97.9377),
        new("oak22",     "22",   "Oak",        "St",   null,                        30.0623, -97.9442),
        new("quickstop", "1800", "Commerce",   "St",   "QuickStop Convenience #14", 30.0701, -97.9520, PreDirection: "W"),
        new("park",      "950",  "River",      "Rd",   "Riverside Park",            30.0589, -97.9351),
        new("mall",      "3100", "Northgate",  "Blvd", "Northgate Mall",            30.0874, -97.9289),
        new("elm415",    "415",  "Elm",        "St",   "Elm Street Apartments",     30.0640, -97.9468, Apt: "3B"),
        new("taproom",   "77",   "Center",     "St",   "The Tap Room",              30.0659, -97.9398),
        new("storage",   "1200", "Industrial", "Blvd", "Riverbend Self Storage",    30.0782, -97.9611),
        new("oak-center", null,  "Oak St & Center St", null, null,                  30.0631, -97.9401),
        new("pecan610",  "610",  "Pecan",      "Dr",   null,                        30.0557, -97.9284),
        new("willow1450","1450", "Willow",     "Way",  null,                        30.0729, -97.9187),
        new("lakeview88","88",   "Lakeview",   "Ct",   null,                        30.0815, -97.9452),
    ];

    private static readonly PersonSeed[] People =
    [
        new("carter",   "Carter",   "Marcus",  "James",   "M", "B", new(1994, 3, 12),  71, 185, "BLK", "BRN", "41872256", "willow1450", "512-555-0142"),
        new("alvarez",  "Alvarez",  "Sofia",   "Isabel",  "F", "H", new(1988, 7, 22),  64, 135, "BRN", "BRN", "30219874", "pecan610",   "512-555-0178"),
        new("nguyen",   "Nguyen",   "Daniel",  "Minh",    "M", "A", new(1979, 11, 2),  68, 160, "BLK", "BRN", "27736410", "lakeview88", "512-555-0115"),
        new("brooks",   "Brooks",   "Hannah",  "Lee",     "F", "W", new(2001, 5, 30),  66, 140, "BLD", "BLU", "45590123", "elm415",     "512-555-0193"),
        new("hollis",   "Hollis",   "Raymond", "Earl",    "M", "W", new(1967, 1, 18),  70, 210, "GRY", "BLU", null,       null,         null),
        new("whitaker", "Whitaker", "Eleanor", "Mae",     "F", "W", new(1941, 9, 9),   62, 120, "WHT", "HAZ", null,       "oak22",      "512-555-0107"),
        new("reyes",    "Reyes",    "Anthony", "Michael", "M", "H", new(1998, 12, 4),  69, 175, "BLK", "BRN", "48812037", "elm415",     null),
        new("patel",    "Patel",    "Priya",   null,      "F", "A", new(1990, 2, 14),  63, 125, "BLK", "BRN", "39904418", null,         "512-555-0166"),
        new("doyle",    "Doyle",    "Kevin",   "Patrick", "M", "W", new(1985, 6, 27),  73, 230, "RED", "GRN", "33671205", null,         "512-555-0121"),
        new("simmons",  "Simmons",  "Tyler",   "Ray",     "M", "B", new(1996, 8, 19),  72, 190, "BLK", "BRN", "46120987", null,         "512-555-0154"),
        new("foster",   "Foster",   "Jessica", "Lynn",    "F", "W", new(2004, 10, 11), 65, 130, "BRN", "GRN", "50017733", null,         null),
        new("kim",      "Kim",      "Jason",   "Ho",      "M", "A", new(1975, 4, 3),   67, 165, "BLK", "BRN", "25548861", null,         "512-555-0139"),
        new("morales",  "Morales",  "Luis",    "Antonio", "M", "H", new(2003, 1, 25),  68, 155, "BLK", "BRN", null,       null,         null),
        new("bennett",  "Bennett",  "Chloe",   "Anne",    "F", "W", new(2006, 3, 8),   64, 120, "BLD", "BLU", "52230419", null,         null),
    ];

    private static readonly IncidentSeed[] IncidentSeeds =
    [
        new("burglary", "26-004512", "CFS-26-018834",
            "Burglary of a business: rear door forced overnight at Maple Avenue Pharmacy; register drawer and OTC merchandise taken.",
            "pharmacy", Days(6) + Hours(3), ["Burglary"],
            "On the listed date at approximately 0640 hrs, I (Ofc. J. Ramirez #214) responded to Maple Avenue Pharmacy, 4521 Maple Ave, regarding a burglary discovered at opening. " +
            "Store manager Sofia Alvarez met me at the front entrance and advised the rear service door had been forced open overnight.\n\n" +
            "I observed pry marks on the door frame consistent with a flat-bladed tool. The cash register drawer (approx. $240) and several boxes of OTC medication were missing. " +
            "Exterior camera footage shows a male in a dark hooded sweatshirt entering at 0247 hrs and leaving at 0258 hrs carrying a black duffel bag.\n\n" +
            "Latent prints were lifted from the register and submitted to the lab. A still from the footage was distributed to patrol. " +
            "Follow-up: see linked arrest. A warrant was obtained after the lab matched the prints to Marcus Carter.",
            RecordStatus.Open),

        new("collision", "26-004377", "CFS-26-018290",
            "Two-vehicle collision at 5th St & Main St. Minor damage, no injuries. One driver unable to provide proof of insurance.",
            "5th-main", Days(12) + Hours(7), [],
            "I (Ofc. M. Chen #231) responded to a two-vehicle collision at the intersection of 5th St and Main St. " +
            "Vehicle 1 (Daniel Nguyen) was northbound on Main St with a green signal. Vehicle 2 (Hannah Brooks) was turning left from 5th St and failed to yield.\n\n" +
            "Both drivers declined EMS. Damage was limited to V1's front bumper and V2's passenger door. Both vehicles were driven from the scene.\n\n" +
            "Brooks could not provide proof of financial responsibility and was cited (see linked citation). Information was exchanged between the parties.",
            RecordStatus.Closed),

        new("trespass", "26-004421", "CFS-26-018502",
            "Trespass complaint at Riverbend Public Library. Subject identified and issued a criminal trespass warning.",
            "library", Days(9) + Hours(5), ["Criminal Trespass"],
            "Library staff requested assistance with a male who had been asked to leave at closing and refused. " +
            "I (Ofc. J. Ramirez #214) contacted the subject, identified as Raymond Hollis, in the reading room.\n\n" +
            "Hollis was cooperative once contacted. At the request of the branch manager I issued a written criminal trespass warning for the property, valid for one year, " +
            "and Hollis left without incident. No arrest was made.",
            RecordStatus.Closed),

        new("welfare", "26-004298", "CFS-26-017964",
            "Welfare check on an elderly resident at 22 Oak St at the request of family. Resident contacted and well.",
            "oak22", Days(15) + Hours(2), [],
            "Dispatch received a call from the resident's daughter, who had been unable to reach her mother by phone for two days. " +
            "I (Sgt. L. Thompson #108) responded to 22 Oak St and made contact with Eleanor Whitaker at the front door.\n\n" +
            "Mrs. Whitaker was alert and in good health and explained that her phone had stopped working. I let her use my phone to call her daughter and provided the number for the senior services line. No further action required.",
            RecordStatus.Closed),

        new("robbery", "26-004588", "CFS-26-019127",
            "Robbery at QuickStop Convenience #14: suspect implied a weapon and demanded cash, fled on foot, and was apprehended nearby.",
            "quickstop", Hours(7), ["Robbery", "Evading Arrest on Foot"],
            "Dispatched to a robbery just occurred at QuickStop Convenience #14, 1800 W Commerce St. " +
            "Clerk Priya Patel reported that a male approached the counter with his hand in his jacket pocket, said he had a gun, and demanded the money from the register. She complied and handed over approximately $180.\n\n" +
            "The suspect fled westbound on foot. Responding units located a male matching the description two blocks west. He ran when ordered to stop and was detained after a short foot pursuit.\n\n" +
            "No weapon was found. Patel positively identified the suspect, Anthony Reyes, in a field show-up. Cash matching the amount taken was recovered from his jacket. See linked arrest.",
            RecordStatus.Open),

        new("taproom", "26-004530", "CFS-26-018911",
            "Disturbance at The Tap Room: fight between two patrons. One arrested for assault; the other cited for public intoxication.",
            "taproom", Days(4) + Hours(1), ["Assault Causing Bodily Injury", "Public Intoxication"],
            "Responded with two other units to a fight in progress at The Tap Room, 77 Center St. On arrival, staff had separated two male patrons.\n\n" +
            "Witnesses and bar staff stated Kevin Doyle struck Tyler Simmons in the face after an argument over a pool game, causing a laceration above his left eye. Simmons declined EMS transport.\n\n" +
            "Doyle was arrested for assault (see linked arrest). Simmons was unsteady, had slurred speech and a strong odor of alcohol, and was cited for public intoxication and released to a sober friend.",
            RecordStatus.Closed),

        new("shoplift", "26-004571", "CFS-26-019050",
            "Shoplifting at Northgate Mall: loss prevention detained a subject with concealed merchandise valued at $64.",
            "mall", Days(1) + Hours(2), ["Theft Under $100"],
            "Northgate Mall loss prevention detained a female who concealed two cosmetics items and a phone case (total retail $64.18) in her purse and walked past the last point of sale.\n\n" +
            "I (Ofc. M. Chen #231) reviewed the store's video, which showed the concealment. The subject, Jessica Foster, admitted taking the items. The merchandise was recovered undamaged.\n\n" +
            "The store requested prosecution. Foster was cited for theft under $100 and issued a trespass warning by mall management.",
            RecordStatus.Open),

        new("vehicle-theft", "26-004595", "CFS-26-019144",
            "Motor vehicle theft reported at Riverbend Self Storage: 2019 Ford F-150 taken from the gated lot overnight. Report in progress.",
            "storage", Hours(3), ["Motor Vehicle Theft"],
            "Owner Jason Kim reported that his white 2019 Ford F-150 was taken from the fenced lot at Riverbend Self Storage overnight. The gate keypad log shows no unusual codes. " +
            "The vehicle was entered into NCIC as stolen. Facility video has been requested. (Draft: pending video review.)",
            RecordStatus.Draft),

        new("harassment", "26-004466", "CFS-26-018677",
            "Harassment complaint: resident at Elm Street Apartments reports repeated unwanted messages from a former acquaintance.",
            "elm415", Days(7) + Hours(4), ["Harassment"],
            "The complainant, a resident of Elm Street Apartments, unit 3B, reports receiving more than 40 unwanted text messages and calls over the past week from a former acquaintance after asking them to stop.\n\n" +
            "Screenshots were collected and attached to the case file. The complainant was advised of the protective order process. The case has been forwarded to investigations for follow-up.",
            RecordStatus.Open),

        new("park", "26-004102", "CFS-26-017201",
            "Narcotics: subject found in possession of marijuana and paraphernalia in Riverside Park after hours.",
            "park", Days(20) + Hours(6), ["Possession of Marijuana < 2 oz", "Possession of Drug Paraphernalia"],
            "While on patrol in Riverside Park after posted closing hours, I (Ofc. J. Ramirez #214) contacted a male seated in a parked vehicle near the boat ramp. A strong odor of marijuana was coming from the vehicle.\n\n" +
            "The driver, Tyler Simmons, admitted to having marijuana. A search revealed a plastic bag of a green leafy substance (field weight 0.6 oz) and a glass pipe. " +
            "Simmons was arrested (see linked arrest). The evidence was tagged and submitted to the property room.",
            RecordStatus.Closed),
    ];

    private static readonly ArrestSeed[] ArrestSeeds =
    [
        new("carter-warrant", "carter", "burglary", "willow1450", "Warrant", Days(5) + Hours(2), ["Burglary"],
            "Served the burglary warrant at the arrestee's residence, 1450 Willow Way. Marcus Carter answered the door and was taken into custody without incident. " +
            "A black duffel bag matching the one in the store footage was in plain view in the living room and was seized as evidence.",
            Close: false),
        new("reyes-robbery", "reyes", "robbery", "quickstop", "OnView", Hours(6), ["Robbery", "Evading Arrest on Foot"],
            "Arrestee fled on foot when ordered to stop and was detained after a two-block foot pursuit. He was positively identified by the victim in a field show-up. " +
            "$180 in mixed bills was recovered from his jacket pocket. He was transported to the county jail.",
            Close: false),
        new("doyle-assault", "doyle", "taproom", "taproom", "ProbableCause", Days(4), ["Assault Causing Bodily Injury"], null, Close: true),
        new("simmons-park", "simmons", "park", "park", "OnView", Days(20) + Hours(5), ["Possession of Marijuana < 2 oz", "Possession of Drug Paraphernalia"], null, Close: true),
    ];

    private static readonly CitationSeed[] CitationSeeds =
    [
        new("brooks-insurance", "brooks", "collision", "5th-main", Days(12) + Hours(6),
            "No proof of financial responsibility after a collision at 5th St & Main St.", ["No Insurance (FMFR)"]),
        new("simmons-pi", "simmons", "taproom", "taproom", Days(4),
            "Public intoxication: subject unsteady with slurred speech following a disturbance at The Tap Room.", ["Public Intoxication"]),
        new("foster-theft", "foster", "shoplift", "mall", Days(1) + Hours(1),
            "Theft of merchandise valued at $64.18 from a Northgate Mall retailer.", ["Theft Under $100"]),
        new("morales-stop", "morales", null, "oak-center", Days(5) + Hours(9),
            "Traffic stop for a stop sign violation at Oak St & Center St. Driver unlicensed and uninsured.", ["Driving Without License", "No Insurance (FMFR)"]),
        new("bennett-mip", "bennett", null, "park", Days(10) + Hours(3),
            "Minor in possession of alcohol at Riverside Park after hours.", ["Minor in Possession of Alcohol"]),
        new("doyle-container", "doyle", null, "taproom", Days(18) + Hours(2),
            "Open container of alcohol in a vehicle parked on Center St outside The Tap Room.", ["Open Container"]),
    ];

    // ------------------------------------------------------------------
    // Seeding
    // ------------------------------------------------------------------

    private sealed class Seeder(IServiceProvider sp, AppDbContext db, ILogger logger)
    {
        private readonly ISender _sender = sp.GetRequiredService<ISender>();
        private readonly IPicklistService _picklists = sp.GetRequiredService<IPicklistService>();
        private readonly DateTime _now = DateTime.UtcNow;
        private readonly Dictionary<string, Dictionary<string, Guid>> _picklistIds = new();
        private Dictionary<string, Guid> _chargeIds = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Guid> Locations { get; } = new();
        public Dictionary<string, Guid> Names { get; } = new();
        public Dictionary<string, (Guid Id, long RecordNumber)> Incidents { get; } = new();
        public Dictionary<string, Guid> Arrests { get; } = new();
        public Dictionary<string, Guid> Citations { get; } = new();
        public int Failures { get; private set; }

        public async Task RunAsync()
        {
            await Step("seed picklists", SeedPicklistsAsync);
            await Step("seed charges", SeedChargesAsync);
            await LoadLookupsAsync();

            foreach (var place in Places)
                await Step($"location {place.Key}", () => CreateLocationAsync(place));
            foreach (var person in People)
                await Step($"name {person.Key}", () => CreateNameAsync(person));
            foreach (var incident in IncidentSeeds)
                await Step($"incident {incident.Key}", () => CreateIncidentAsync(incident));
            foreach (var arrest in ArrestSeeds)
                await Step($"arrest {arrest.Key}", () => CreateArrestAsync(arrest));
            foreach (var citation in CitationSeeds)
                await Step($"citation {citation.Key}", () => CreateCitationAsync(citation));

            await ApplyLifecycleAsync();
            await Step("remove legacy sample records", RemoveLegacySampleRecordsAsync);
        }

        private async Task Step(string what, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Failures++;
                logger.LogWarning(ex, "Demo dataset: could not {Step}.", what);
            }
        }

        // ---- Reference data ----

        private async Task SeedPicklistsAsync()
        {
            // System defaults for every picklist type that has them (arrest types,
            // street types, directions, violation codes, ...).
            var types = typeof(PicklistTypes)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!);
            foreach (var type in types)
                await _sender.Send(new SeedDefaultPicklistItemsCommand(type));

            // Person descriptors and states, which the Test Data page seeds manually.
            await EnsurePicklistAsync(PicklistTypes.Sex, FakeNameData.SexDefaults);
            await EnsurePicklistAsync(PicklistTypes.Race, FakeNameData.RaceDefaults);
            await EnsurePicklistAsync(PicklistTypes.HairColor, FakeNameData.HairColorDefaults);
            await EnsurePicklistAsync(PicklistTypes.EyeColor, FakeNameData.EyeColorDefaults);
            await EnsurePicklistAsync(PicklistTypes.Suffix, FakeNameData.SuffixDefaults);
            await EnsurePicklistAsync(PicklistTypes.State,
                FakeNameData.States.Select((s, i) => (s.Value, s.Label, i + 1)).ToArray());
        }

        private async Task EnsurePicklistAsync(string type, (string Value, string Label, int SortOrder)[] defaults)
        {
            var existing = (await _picklists.GetItemsAsync(type, activeOnly: false))
                .Select(x => x.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var (value, label, sort) in defaults)
                if (!existing.Contains(value))
                    await _picklists.CreateItemAsync(type, value, label, sort);
        }

        private async Task SeedChargesAsync()
        {
            if (await db.Charges.AnyAsync()) return;

            var path = ChargeSeedFile.Locate(sp.GetRequiredService<IWebHostEnvironment>())
                ?? throw new FileNotFoundException("charges.json seed file not found.");
            await _sender.Send(new SeedChargesCommand(await File.ReadAllTextAsync(path)));
        }

        private async Task LoadLookupsAsync()
        {
            foreach (var type in new[]
                     {
                         PicklistTypes.Sex, PicklistTypes.Race, PicklistTypes.HairColor, PicklistTypes.EyeColor,
                         PicklistTypes.State, PicklistTypes.StreetType, PicklistTypes.Direction,
                         PicklistTypes.Country, PicklistTypes.ArrestType,
                     })
            {
                _picklistIds[type] = (await _picklists.GetItemsAsync(type, activeOnly: false))
                    .GroupBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
            }

            _chargeIds = (await db.Charges.AsNoTracking().Select(c => new { c.Id, c.OffenseName }).ToListAsync())
                .GroupBy(c => c.OffenseName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
        }

        private Guid? Pick(string type, string? value)
            => value is not null && _picklistIds.TryGetValue(type, out var ids) && ids.TryGetValue(value, out var id)
                ? id
                : null;

        private List<Guid> ChargeIds(IEnumerable<string> names)
        {
            var ids = new List<Guid>();
            foreach (var name in names)
            {
                if (_chargeIds.TryGetValue(name, out var id)) ids.Add(id);
                else logger.LogWarning("Demo dataset: charge '{Charge}' not found in the catalog.", name);
            }
            return ids;
        }

        private DateTime Ago(TimeSpan ago) => _now - ago;

        // ---- Records ----

        private async Task CreateLocationAsync(PlaceSeed p)
        {
            var display = p.Number is null
                ? $"{p.Street}, {City}, {StateCode} {Zip}"
                : $"{p.PreDirection}{(p.PreDirection is null ? "" : " ")}{p.Number} {p.Street} {p.StreetType}{(p.Apt is null ? "" : $" #{p.Apt}")}, {City}, {StateCode} {Zip}";

            var recordNumber = await _sender.Send(new CreateLocationCommand(
                StreetAddress: p.Street,
                City: City,
                StreetNumber: p.Number,
                PreDirectionId: Pick(PicklistTypes.Direction, p.PreDirection),
                StreetTypeId: Pick(PicklistTypes.StreetType, p.StreetType),
                StateId: Pick(PicklistTypes.State, StateCode),
                CountryId: Pick(PicklistTypes.Country, "US"),
                Zip: Zip,
                AptSuite: p.Apt,
                Coordinates: FormattableString.Invariant($"{p.Lat},{p.Lng}"),
                CommonPlaceName: p.PlaceName,
                Address: display));

            Locations[p.Key] = await db.Locations.AsNoTracking()
                .Where(l => l.RecordNumber == recordNumber).Select(l => l.Id).SingleAsync();
        }

        private async Task CreateNameAsync(PersonSeed p)
        {
            var dlState = p.Dl is null ? null : Pick(PicklistTypes.State, StateCode);
            var recordNumber = await _sender.Send(new CreateNameCommand(
                NameType: NameTypes.Person,
                LastOrBusinessName: p.Last,
                FirstName: p.First,
                MiddleName: p.Middle,
                SexId: Pick(PicklistTypes.Sex, p.Sex),
                RaceId: Pick(PicklistTypes.Race, p.Race),
                DateOfBirth: p.Dob,
                DriversLicenseNumber: p.Dl,
                DriversLicenseStateId: dlState,
                HeightInches: p.Height,
                WeightLbs: p.Weight,
                HairColorId: Pick(PicklistTypes.HairColor, p.Hair),
                EyeColorId: Pick(PicklistTypes.EyeColor, p.Eyes),
                SuffixId: null,
                PlaceOfBirth: null,
                FbiNumber: null,
                LocalNumber: null,
                SocialSecurityNumber: null,
                IsCitizen: true,
                DeceasedDate: null,
                PrimaryPhone: p.Phone));

            var nameId = await db.Names.AsNoTracking()
                .Where(n => n.RecordNumber == recordNumber).Select(n => n.Id).SingleAsync();
            Names[p.Key] = nameId;

            // Home address: names are lockable, so take and release the lock the
            // same way the edit page does.
            if (p.HomeKey is not null && Locations.TryGetValue(p.HomeKey, out var homeId))
            {
                await _sender.Send(new AcquireNameLockCommand(nameId));
                try
                {
                    await _sender.Send(new UpdateNameDetailsCommand(
                        nameId, NameTypes.Person, p.Last, p.First, p.Middle,
                        Pick(PicklistTypes.Sex, p.Sex), Pick(PicklistTypes.Race, p.Race), p.Dob,
                        p.Dl, dlState, p.Height, p.Weight,
                        Pick(PicklistTypes.HairColor, p.Hair), Pick(PicklistTypes.EyeColor, p.Eyes),
                        SuffixId: null, PlaceOfBirth: null, FbiNumber: null, LocalNumber: null,
                        SocialSecurityNumber: null, IsCitizen: true, DeceasedDate: null,
                        PrimaryLocationId: homeId, PrimaryPhone: p.Phone));
                }
                finally
                {
                    await _sender.Send(new ReleaseNameLockCommand(nameId));
                }
            }
        }

        private async Task CreateIncidentAsync(IncidentSeed s)
        {
            var details = new IncidentDetails
            {
                IncidentNum = s.Num,
                LocalNum = s.Cfs,
                Description = s.Summary,
                CFSNum = s.Cfs,
            };

            var recordNumber = await _sender.Send(new CreateIncidentCommand(details));
            var id = await db.Incidents.AsNoTracking()
                .Where(i => i.RecordNumber == recordNumber).Select(i => i.Id).SingleAsync();
            Incidents[s.Key] = (id, recordNumber);

            // Drafts can be edited without a lock.
            await _sender.Send(new SaveIncidentPageCommand(
                id, details,
                LocationId: Locations[s.PlaceKey],
                OccurredOn: Ago(s.Ago),
                ChargeIdsToAdd: ChargeIds(s.Charges)));

            await _sender.Send(new CreateNarrativeCommand(NarrativeOwnerTypes.Incident, id, "Initial report", s.Narrative));
        }

        private async Task CreateArrestAsync(ArrestSeed s)
        {
            var incident = Incidents[s.IncidentKey];
            var nameId = Names[s.PersonKey];
            var locationId = Locations[s.PlaceKey];
            var arrestedAt = Ago(s.Ago);

            var recordNumber = await _sender.Send(new CreateArrestCommand(
                NameId: nameId,
                ArrestedAt: arrestedAt,
                IncidentRecordNumbers: [incident.RecordNumber],
                PrimaryIncidentId: incident.Id,
                LocationId: locationId));

            var arrest = await db.Arrests.AsNoTracking()
                .Where(a => a.RecordNumber == recordNumber)
                .Select(a => new { a.Id, a.ArrestNum })
                .SingleAsync();
            Arrests[s.Key] = arrest.Id;

            await _sender.Send(new SaveArrestPageCommand(
                arrest.Id, nameId, arrestedAt,
                ArrestTypeId: Pick(PicklistTypes.ArrestType, s.ArrestType),
                ArrestNum: arrest.ArrestNum ?? string.Empty,
                LocationId: locationId,
                PrimaryIncidentId: incident.Id,
                ChargeIdsToAdd: ChargeIds(s.Charges)));

            if (s.Narrative is not null)
                await _sender.Send(new CreateNarrativeCommand(NarrativeOwnerTypes.Arrest, arrest.Id, "Arrest narrative", s.Narrative));
        }

        private async Task CreateCitationAsync(CitationSeed s)
        {
            var issuedAt = Ago(s.Ago);
            var incidentNumbers = s.IncidentKey is null ? new List<long>() : [Incidents[s.IncidentKey].RecordNumber];

            var recordNumber = await _sender.Send(new CreateCitationCommand(s.Description, issuedAt, incidentNumbers));
            var citation = await db.Citations.AsNoTracking()
                .Where(c => c.RecordNumber == recordNumber)
                .Select(c => new { c.Id, c.CitationNum })
                .SingleAsync();
            Citations[s.Key] = citation.Id;

            await _sender.Send(new SaveCitationPageCommand(
                citation.Id,
                DefendantNameId: Names[s.PersonKey],
                Description: s.Description,
                IssueDate: issuedAt,
                CourtId: null,
                CitationNum: citation.CitationNum ?? string.Empty,
                LocationId: Locations[s.PlaceKey],
                ChargeIdsToAdd: ChargeIds(s.Charges)));

            await _sender.Send(new IssueCitationCommand(citation.Id));
        }

        // ---- Statuses ----

        private async Task ApplyLifecycleAsync()
        {
            foreach (var s in ArrestSeeds.Where(a => Arrests.ContainsKey(a.Key)))
            {
                var id = Arrests[s.Key];
                await Step($"open arrest {s.Key}", () => _sender.Send(new OpenArrestCommand(id)));
                if (!s.Close) continue;
                await Step($"finalize arrest {s.Key}", () => _sender.Send(new FinalizeArrestCommand(id)));
                await Step($"close arrest {s.Key}", () => _sender.Send(new CloseArrestCommand(id)));
            }

            foreach (var s in IncidentSeeds.Where(i => Incidents.ContainsKey(i.Key) && i.Status != RecordStatus.Draft))
            {
                var id = Incidents[s.Key].Id;
                await Step($"open incident {s.Key}", () => _sender.Send(new OpenIncidentCommand(id)));
                if (s.Status == RecordStatus.Closed)
                    await Step($"close incident {s.Key}", () => _sender.Send(new CloseIncidentCommand(id)));
            }
        }

        /// <summary>
        /// Older builds seeded a handful of bare placeholder records (no people,
        /// places or charges). Soft-delete them so the demo shows only this dataset.
        /// </summary>
        private async Task RemoveLegacySampleRecordsAsync()
        {
            var incidents = await db.Incidents.AsNoTracking()
                .Where(i => LegacyIncidentNums.Contains(i.IncidentNum)).Select(i => i.Id).ToListAsync();
            foreach (var id in incidents)
                await _sender.Send(new SoftDeleteIncidentCommand(id));

            var arrests = await db.Arrests.AsNoTracking()
                .Where(a => LegacyArrestNums.Contains(a.ArrestNum)).Select(a => a.Id).ToListAsync();
            foreach (var id in arrests)
                await _sender.Send(new SoftDeleteArrestCommand(id));

            var citations = await db.Citations.AsNoTracking()
                .Where(c => LegacyCitationNums.Contains(c.CitationNum)).Select(c => c.Id).ToListAsync();
            foreach (var id in citations)
                await _sender.Send(new SoftDeleteCitationCommand(id));
        }
    }
}

/// <summary>Finds the charge catalog seed file shipped with the app.</summary>
internal static class ChargeSeedFile
{
    public static string? Locate(IWebHostEnvironment env)
    {
        string[] candidates =
        [
            // Published / Docker: copied next to the app (see the UI .csproj).
            Path.Combine(AppContext.BaseDirectory, "SeedData", "charges.json"),
            // Running from the source tree.
            Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Documents", "charges.json")),
        ];
        return candidates.FirstOrDefault(File.Exists);
    }
}
