namespace Modules.Records.Application.TestData;

public static class TestDataFakeNameGenerator
{
    private static readonly string[] MaleFirstNames =
    [
        "James", "John", "Robert", "Michael", "William", "David", "Joseph", "Thomas", "Charles", "Daniel",
        "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua", "Kevin", "Brian",
        "George", "Timothy", "Edward", "Jason", "Ryan", "Jacob", "Nicholas", "Jonathan", "Justin", "Benjamin"
    ];

    private static readonly string[] FemaleFirstNames =
    [
        "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Susan", "Jessica", "Sarah", "Karen", "Nancy",
        "Lisa", "Sandra", "Ashley", "Emily", "Donna", "Michelle", "Amanda", "Melissa", "Rebecca", "Sharon",
        "Laura", "Amy", "Angela", "Anna", "Emma", "Nicole", "Samantha", "Katherine", "Rachel", "Olivia"
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
        "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"
    ];

    private static readonly string[] MiddleNames =
    [
        "James", "Lee", "Ann", "Lynn", "Marie", "Grace", "Rose", "Jean", "Ray", "Wayne",
        "Allen", "Scott", "Nicole", "Renee", "Louise", "Kay", "Jo", "Dawn", "Faith", "Hope"
    ];

    public static string GenerateSex()
    {
        var roll = Random.Shared.Next(100);
        return roll < 49 ? "M" : roll < 98 ? "F" : "U";
    }

    public static string GenerateRaceValue()
    {
        var roll = Random.Shared.Next(1000);
        return roll switch
        {
            < 590 => "W",
            < 780 => "H",
            < 900 => "B",
            < 960 => "A",
            < 970 => "I",
            < 975 => "P",
            < 990 => "M",
            _ => "U"
        };
    }

    public static DateTime GenerateDob()
    {
        int minAge;
        int maxAge;
        var bucket = Random.Shared.Next(100);

        if (bucket < 2)
        {
            minAge = 0;
            maxAge = 3;
        }
        else if (bucket < 12)
        {
            minAge = 4;
            maxAge = 15;
        }
        else if (bucket < 72)
        {
            minAge = 16;
            maxAge = 55;
        }
        else if (bucket < 97)
        {
            minAge = 56;
            maxAge = 84;
        }
        else
        {
            minAge = 85;
            maxAge = 100;
        }

        var ageDays = Random.Shared.Next(minAge * 365, maxAge * 365 + 364);
        return DateTime.Today.AddDays(-ageDays);
    }

    public static int GetAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob.AddYears(age) > today)
        {
            age--;
        }

        return age;
    }

    public static string GenerateDlNumber()
    {
        return Random.Shared.Next(4) switch
        {
            0 => $"{RandLetter()}{RandDigits(7)}",
            1 => $"{RandLetter()}{RandDigits(8)}",
            2 => $"{RandLetter()}{RandLetter()}{RandDigits(6)}",
            _ => RandDigits(9)
        };
    }

    public static int GenerateHeight(string sex)
    {
        return sex switch
        {
            "M" => Clamp(NormalApprox(69, 3), 60, 84),
            "F" => Clamp(NormalApprox(64, 3), 56, 78),
            _ => Clamp(NormalApprox(66, 4), 56, 84)
        };
    }

    public static int GenerateWeight(string sex)
    {
        return sex switch
        {
            "M" => Clamp(NormalApprox(190, 30), 120, 320),
            "F" => Clamp(NormalApprox(165, 28), 100, 270),
            _ => Clamp(NormalApprox(178, 32), 100, 320)
        };
    }

    public static string PickFirstName(string sex)
    {
        return sex == "M"
            ? MaleFirstNames[Random.Shared.Next(MaleFirstNames.Length)]
            : sex == "F"
                ? FemaleFirstNames[Random.Shared.Next(FemaleFirstNames.Length)]
                : (Random.Shared.Next(2) == 0
                    ? MaleFirstNames[Random.Shared.Next(MaleFirstNames.Length)]
                    : FemaleFirstNames[Random.Shared.Next(FemaleFirstNames.Length)]);
    }

    public static string PickLastName() => LastNames[Random.Shared.Next(LastNames.Length)];

    public static string? PickMiddleName()
    {
        if (Random.Shared.Next(10) >= 7)
        {
            return null;
        }

        if (Random.Shared.Next(5) == 0)
        {
            return ((char)('A' + Random.Shared.Next(26))).ToString();
        }

        return MiddleNames[Random.Shared.Next(MiddleNames.Length)];
    }

    public static T? PickRandom<T>(IReadOnlyList<T> items) where T : struct
    {
        if (items.Count == 0)
        {
            return null;
        }

        return items[Random.Shared.Next(items.Count)];
    }

    public static T PickRandomRequired<T>(IReadOnlyList<T> items)
        => items[Random.Shared.Next(items.Count)];

    private static char RandLetter() => (char)('A' + Random.Shared.Next(26));

    private static string RandDigits(int count)
        => string.Concat(Enumerable.Range(0, count).Select(_ => Random.Shared.Next(10).ToString()));

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;

    private static int NormalApprox(double mean, double stdDev)
    {
        double sum = 0;
        for (var i = 0; i < 6; i++)
        {
            sum += Random.Shared.NextDouble();
        }

        var z = (sum - 3.0) / 0.5;
        return (int)Math.Round(mean + (z * stdDev));
    }
}
