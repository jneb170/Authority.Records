using Modules.Records.Application.TestData;

namespace Modules.Records.UI.Helpers;

public static class FakeNameGenerator
{
    public static string GenerateSex() => TestDataFakeNameGenerator.GenerateSex();

    public static string GenerateRaceValue() => TestDataFakeNameGenerator.GenerateRaceValue();

    public static DateTime GenerateDob() => TestDataFakeNameGenerator.GenerateDob();

    public static int GetAge(DateTime dob) => TestDataFakeNameGenerator.GetAge(dob);

    public static string GenerateDlNumber() => TestDataFakeNameGenerator.GenerateDlNumber();

    public static int GenerateHeight(string sex) => TestDataFakeNameGenerator.GenerateHeight(sex);

    public static int GenerateWeight(string sex) => TestDataFakeNameGenerator.GenerateWeight(sex);

    public static string PickFirstName(string sex) => TestDataFakeNameGenerator.PickFirstName(sex);

    public static string PickLastName() => TestDataFakeNameGenerator.PickLastName();

    public static string? PickMiddleName() => TestDataFakeNameGenerator.PickMiddleName();

    public static T? PickRandom<T>(IReadOnlyList<T> items) where T : struct
        => TestDataFakeNameGenerator.PickRandom(items);

    public static T PickRandomRequired<T>(IReadOnlyList<T> items)
        => TestDataFakeNameGenerator.PickRandomRequired(items);
}
