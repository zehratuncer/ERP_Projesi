namespace ERP.Domain.Enums;

public enum PaymentMethod
{
    Cash = 1,        // Nakit
    CreditCard = 2,  // Kredi Kartı
    Split = 3,       // Parçalı Ödeme (Nakit + Kart)
    OnAccount = 4    // Veresiye / Cari Hesap
}
