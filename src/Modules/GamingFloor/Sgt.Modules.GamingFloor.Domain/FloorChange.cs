using Sgt.BuildingBlocks;

namespace Sgt.Modules.GamingFloor.Domain;

public enum GameType
{
    Blackjack,
    Roulette,
    Poker,
    Baccarat,
    Other,
}

public enum FloorChangeKind
{
    TableDefined,
    TableOpened,
    TableClosed,
    RatingStarted,
    RatingFinished,
    RatingCorrected,
    SlotMachineDefined,
    SlotReadingEntered,
    SlotReadingsImported,
}

public sealed record FloorChange(Guid Id, FloorChangeKind Kind, string Detail, AuditContext Audit, DateTimeOffset OccurredAt);
