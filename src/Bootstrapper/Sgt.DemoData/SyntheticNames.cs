using System.Globalization;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.DemoData;

/// <summary>Tamamen sentetik oyuncu bilgisi (Spec 012 KK7). Gerçek kişiye ait değildir.</summary>
public sealed record SyntheticPerson(
    string FirstName, string LastName, DateOnly BirthDate, string Nationality, DocumentType DocumentType, string DocumentNumber, string? Phone);

/// <summary>
/// Sabit tohumlu isim ve belge üretici (Spec 012 KK5, KK7). İsimler yaygın ad/soyad listelerinden rastgele eşlenir;
/// belge numaraları "DM"/"DP" önekli ve sıra numaralıdır, gerçek belge biçimine (ör. TC kimlik) uymaz.
/// </summary>
public sealed class SyntheticNames(int seed)
{
    private const int MinAge = 21;
    private const int AgeSpan = 55;

    private static readonly NameSet[] Sets =
    [
        new("TR", 40, ["Ahmet", "Mehmet", "Ayşe", "Fatma", "Mustafa", "Emine", "Ali", "Zeynep", "Hüseyin", "Elif", "Murat", "Selin", "Emre", "Deniz", "Burak", "Ece"],
            ["Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Aydın", "Öztürk", "Arslan", "Doğan", "Kılıç", "Aslan", "Koç", "Kurt", "Özdemir", "Polat"]),
        new("CY", 15, ["Hasan", "Mine", "Erol", "Gül", "Kemal", "Nevin", "Sinan", "Derya", "Taner", "Pembe"],
            ["Hüseyinoğlu", "Mehmetoğlu", "Alioğlu", "Ozanlı", "Mertoğlu", "Erdenli", "Tatar", "Karpaz", "Lefkeli", "Girneli"]),
        new("GB", 20, ["James", "Oliver", "Emily", "Sophie", "Harry", "Charlotte", "George", "Amelia", "Jack", "Grace", "Thomas", "Lucy"],
            ["Smith", "Jones", "Taylor", "Brown", "Wilson", "Evans", "Thomas", "Roberts", "Walker", "Wright", "Green", "Hall"]),
        new("RU", 20, ["Ivan", "Dmitry", "Olga", "Anna", "Sergey", "Elena", "Alexei", "Natalia", "Pavel", "Irina", "Mikhail", "Tatiana"],
            ["Ivanov", "Smirnov", "Kuznetsov", "Popov", "Sokolov", "Lebedev", "Kozlov", "Novikov", "Morozov", "Volkov", "Orlov", "Zaitsev"]),
        new("DE", 5, ["Lukas", "Anna", "Felix", "Lena", "Jonas", "Marie"], ["Müller", "Schmidt", "Schneider", "Fischer", "Weber", "Becker"]),
    ];

    private static readonly int TotalWeight = Sets.Sum(s => s.Weight);

#pragma warning disable CA5394 // Güvenlik amacı yok: demo verisi için tekrarlanabilir (sabit tohumlu) rastgelelik gerekir.
    private readonly Random _random = new(seed);
    private int _sequence;

    public SyntheticPerson NextPerson(DateOnly today)
    {
        var set = PickSet();
        var first = set.FirstNames[_random.Next(set.FirstNames.Count)];
        var last = set.LastNames[_random.Next(set.LastNames.Count)];
        var birth = today.AddYears(-(MinAge + _random.Next(AgeSpan))).AddDays(-_random.Next(365));
        var sequence = ++_sequence;

        // Yerli oyuncu kimlik kartı, yabancı oyuncu pasaport ile kaydolur.
        var isLocal = set.Nationality is "TR" or "CY";
        var type = isLocal ? DocumentType.NationalId : DocumentType.Passport;
        var document = (isLocal ? "DM" : "DP") + sequence.ToString("D8", CultureInfo.InvariantCulture);
        var phone = _random.Next(3) == 0 ? null : "+90548" + _random.Next(1_000_000, 9_999_999).ToString(CultureInfo.InvariantCulture);
        return new SyntheticPerson(first, last, birth, set.Nationality, type, document, phone);
    }

    /// <summary>Kart numarası: "SGT" + 8 hane, oyuncu sırasından türetilir (benzersiz).</summary>
    public static string CardNumber(int index) => "SGT" + index.ToString("D8", CultureInfo.InvariantCulture);

    private NameSet PickSet()
    {
        var roll = _random.Next(TotalWeight);
        foreach (var set in Sets)
        {
            if (roll < set.Weight)
            {
                return set;
            }

            roll -= set.Weight;
        }

        return Sets[0];
    }
#pragma warning restore CA5394

    private sealed record NameSet(string Nationality, int Weight, IReadOnlyList<string> FirstNames, IReadOnlyList<string> LastNames);
}
