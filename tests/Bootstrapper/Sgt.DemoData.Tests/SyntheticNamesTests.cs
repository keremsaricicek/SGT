namespace Sgt.DemoData.Tests;

public sealed class SyntheticNamesTests
{
    private static readonly DateOnly Today = new(2026, 11, 21);

    [Fact]
    public void Same_seed_produces_same_people()
    {
        var a = Take(new SyntheticNames(42), 50);
        var b = Take(new SyntheticNames(42), 50);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Different_seed_produces_different_people()
    {
        Assert.NotEqual(Take(new SyntheticNames(1), 20), Take(new SyntheticNames(2), 20));
    }

    [Fact]
    public void People_are_adults_with_valid_fields_and_unique_documents()
    {
        var people = Take(new SyntheticNames(7), 500);

        Assert.All(people, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.FirstName));
            Assert.False(string.IsNullOrWhiteSpace(p.LastName));
            Assert.Matches("^[A-Z]{2}$", p.Nationality);
            Assert.True(p.BirthDate.AddYears(21) <= Today);
            Assert.True(p.BirthDate.AddYears(80) >= Today);
            Assert.Matches("^[A-Z0-9]+$", p.DocumentNumber);
        });
        Assert.Equal(500, people.Select(p => (p.DocumentType, p.DocumentNumber)).Distinct().Count());
        Assert.True(people.Select(p => p.Nationality).Distinct().Count() >= 4, "TR/EN/RU/GB karışık olmalı");
    }

    [Fact]
    public void Card_numbers_are_unique_and_within_card_format()
    {
        var cards = Enumerable.Range(1, 500).Select(SyntheticNames.CardNumber).ToList();

        Assert.Equal(500, cards.Distinct().Count());
        Assert.All(cards, c => Assert.Matches("^[A-Z0-9]{4,20}$", c));
    }

    private static List<SyntheticPerson> Take(SyntheticNames names, int count) =>
        Enumerable.Range(0, count).Select(_ => names.NextPerson(Today)).ToList();
}
