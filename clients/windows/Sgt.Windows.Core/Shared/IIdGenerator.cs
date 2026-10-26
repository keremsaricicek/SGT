namespace Sgt.Windows.Core.Shared;

/// <summary>Kimlik üretimi; testlerde öngörülebilir değer verilebilsin diye arayüz arkasındadır.</summary>
public interface IIdGenerator
{
    Guid NewId();
}

internal sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId() => Guid.NewGuid();
}
