namespace Sgt.BuildingBlocks;

/// <summary>Saat soyutlaması. Domain ve Application DateTime.Now yerine bunu kullanır (K1/DIP).</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
