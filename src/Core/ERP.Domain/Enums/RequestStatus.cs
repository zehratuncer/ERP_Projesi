namespace ERP.Domain.Enums;

public enum RequestStatus
{
    Draft = 1,            // Taslak
    PendingApproval = 2,  // Onay Bekliyor
    Approved = 3,         // Onaylandı
    Rejected = 4,         // Reddedildi
    Completed = 5,        // Tamamlandı / Siparişe Dönüştürüldü
    Cancelled = 6         // İptal Edildi
}
