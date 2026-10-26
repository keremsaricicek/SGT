using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Tests;

public sealed class PlayerTests
{
    internal static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    internal static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    internal static readonly DateOnly Today = new(2026, 10, 26);

    internal static AuditContext Audit(UserId? actor = null) =>
        new(actor ?? new UserId(Guid.Parse("00000000-0000-0000-0000-000000000001")), "RESEPSIYON-1", ReasonCode.Of("PLAYER_DESK"), Guid.NewGuid());

    internal static PlayerDetails Details(DateOnly? birthDate = null, string document = "12345678901") =>
        new("Deneme", "Oyuncu", birthDate ?? new DateOnly(1990, 5, 1), "CY", DocumentType.NationalId, document, "+905330000000");

    internal static Player NewPlayer() =>
        Player.Register(PlayerId.New(), Tenant, PlayerNumber.FromSequence(1), Details(), Today, Audit(), Now).Value;

    [Fact]
    public void KK1_KK3_register_creates_player_with_number_and_records_change()
    {
        var player = NewPlayer();

        Assert.Equal("P000001", player.Number.Value);
        Assert.Equal("CY", player.Details.Nationality);
        Assert.Equal(PlayerChangeKind.Registered, Assert.Single(player.PendingChanges).Kind);
    }

    [Theory]
    [InlineData(2008, 10, 27)] // 18. doğum gününe bir gün var
    [InlineData(2015, 1, 1)]
    public void KK1_under_18_cannot_register(int y, int m, int d)
    {
        var result = Player.Register(PlayerId.New(), Tenant, PlayerNumber.FromSequence(1), Details(new DateOnly(y, m, d)), Today, Audit(), Now);

        Assert.Equal(PlayerErrors.Underage, result.Error);
    }

    [Fact]
    public void KK1_exactly_18_today_can_register()
    {
        Assert.True(Player.Register(PlayerId.New(), Tenant, PlayerNumber.FromSequence(1), Details(new DateOnly(2008, 10, 26)), Today, Audit(), Now).IsSuccess);
    }

    [Theory]
    [InlineData("", "Oyuncu", "CY", "123")]
    [InlineData("Ad", " ", "CY", "123")]
    [InlineData("Ad", "Soyad", "Kıbrıs", "123")]
    [InlineData("Ad", "Soyad", "CY", " ")]
    public void KK1_required_fields_are_validated(string first, string last, string nationality, string document)
    {
        var details = new PlayerDetails(first, last, new DateOnly(1990, 1, 1), nationality, DocumentType.Passport, document, null);

        Assert.True(Player.Register(PlayerId.New(), Tenant, PlayerNumber.FromSequence(1), details, Today, Audit(), Now).IsFailure);
    }

    [Fact]
    public void KK4_document_number_is_normalized_and_masked()
    {
        var player = Player.Register(PlayerId.New(), Tenant, PlayerNumber.FromSequence(1), Details(document: " ab 12-3456 "), Today, Audit(), Now).Value;

        Assert.Equal("AB123456", player.Details.DocumentNumber);
        Assert.Equal("****3456", player.Details.MaskedDocumentNumber);
    }

    [Fact]
    public void KK7_new_card_cancels_the_previous_active_card()
    {
        var player = NewPlayer();

        player.IssueCard(CardNumber.Parse("100200").Value, Audit(), Now);
        player.IssueCard(CardNumber.Parse("100201").Value, Audit(), Now.AddDays(1));

        Assert.Equal("100201", player.ActiveCard!.Number.Value);
        Assert.Single(player.Cards, c => c.IsActive);
        Assert.Equal(2, player.Cards.Count);
    }

    [Fact]
    public void KK7_cancel_requires_an_active_card()
    {
        var player = NewPlayer();

        Assert.Equal(PlayerErrors.NoActiveCard, player.CancelCard(Audit(), Now).Error);
        player.IssueCard(CardNumber.Parse("100200").Value, Audit(), Now);
        Assert.True(player.CancelCard(Audit(), Now).IsSuccess);
        Assert.Null(player.ActiveCard);
    }

    [Fact]
    public void KK8_KK10_restriction_makes_player_restricted_until_it_ends()
    {
        var player = NewPlayer();

        player.AddRestriction(RestrictionType.SelfExcluded, "talep üzerine", Now.AddDays(30), Audit(), Now);

        Assert.True(player.IsRestricted(Now.AddDays(1)));
        Assert.False(player.IsRestricted(Now.AddDays(31)));
    }

    [Fact]
    public void KK9_person_who_added_restriction_cannot_remove_it()
    {
        var player = NewPlayer();
        var adder = UserId.New();
        var id = player.AddRestriction(RestrictionType.Banned, null, null, Audit(adder), Now).Value;

        Assert.Equal(PlayerErrors.FourEyesRequired, player.RemoveRestriction(id, Audit(adder), Now).Error);
        Assert.True(player.RemoveRestriction(id, Audit(UserId.New()), Now).IsSuccess);
        Assert.False(player.IsRestricted(Now));
    }

    [Fact]
    public void KK6_profile_update_revalidates_age()
    {
        var player = NewPlayer();

        Assert.Equal(PlayerErrors.Underage, player.UpdateDetails(Details(new DateOnly(2010, 1, 1)), Today, Audit(), Now).Error);
        Assert.True(player.UpdateDetails(Details() with { Phone = null }, Today, Audit(), Now).IsSuccess);
        Assert.Null(player.Details.Phone);
    }
}
