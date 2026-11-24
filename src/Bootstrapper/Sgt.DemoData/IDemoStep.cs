namespace Sgt.DemoData;

/// <summary>Demo verisinin bir bölümü (kullanıcılar, kurlar, oyuncular, salon, günler). Yeni veri türü = yeni adım (K1/OCP).</summary>
public interface IDemoStep
{
    /// <summary>Kayıt sırası: küçük önce çalışır.</summary>
    int Order { get; }

    string Name { get; }

    Task Run(DemoRun run, CancellationToken ct);
}
