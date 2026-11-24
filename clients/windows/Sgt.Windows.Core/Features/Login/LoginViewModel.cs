using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Auth;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Login;

/// <summary>
/// Giriş ekranı (Y1). Şifre bu pencereye yazılmaz: "Giriş yap" sistem tarayıcısında Keycloak sayfasını açar
/// (şifre + MFA orada), dönüşte ana pencere açılır.
/// </summary>
public sealed partial class LoginViewModel(IAuthService auth, IAppNavigator navigator) : StatusViewModel
{
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task LoginAsync(CancellationToken ct)
    {
        IsBusy = true;
        ShowStatus(Strings.Login_Waiting);
        try
        {
            var outcome = await auth.LoginAsync(ct);
            if (outcome.Succeeded)
            {
                ClearMessages();
                navigator.ShowShell();
            }
            else
            {
                ShowError(outcome.Error ?? Strings.Login_Failed);
            }
        }
        catch (OperationCanceledException)
        {
            ShowStatus(Strings.Login_Cancelled);
        }
        catch (HttpRequestException)
        {
            ShowError(Strings.Login_ServerUnreachable);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
