namespace Sgt.Windows.Core.Shared;

/// <summary>
/// Bu terminalde çalışılan kasiyer vardiyası. Kasa ekranları işlemleri bu vardiyaya yazar.
/// Vardiya açılınca veya mevcut vardiyaya bağlanınca ayarlanır, kapanınca temizlenir.
/// </summary>
public interface ICashierShiftContext
{
    Guid? ShiftId { get; }

    event EventHandler? Changed;

    void Attach(Guid shiftId);

    void Detach();
}

internal sealed class CashierShiftContext : ICashierShiftContext
{
    public Guid? ShiftId { get; private set; }

    public event EventHandler? Changed;

    public void Attach(Guid shiftId)
    {
        ShiftId = shiftId;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Detach()
    {
        ShiftId = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
