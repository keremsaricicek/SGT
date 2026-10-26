using Sgt.Windows.Core.Api;

namespace Sgt.Windows.Core.Features.Shell;

/// <summary>
/// Oturum açılıp yetkiler alındıktan sonra çalışan hazırlık adımı (ör. kasiyerin açık vardiyasına bağlanma).
/// Her özellik kendi adımını kaydeder; kabuk yalnızca sırayla çalıştırır (OCP).
/// </summary>
public interface ISessionInitializer
{
    /// <summary>Başarısızsa kullanıcıya gösterilecek problem; değilse null.</summary>
    Task<ApiProblem?> InitializeAsync(UserAccess access, CancellationToken ct);
}
