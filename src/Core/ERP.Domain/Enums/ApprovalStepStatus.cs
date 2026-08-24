namespace ERP.Domain.Enums;

public enum ApprovalStepStatus
{
    Pending = 1,    // Beklemede
    Approved = 2,   // Onaylandı
    Rejected = 3,   // Reddedildi
    Skipped = 4     // Atlandı (Tutar limiti gerekmediğinde vb.)
}
