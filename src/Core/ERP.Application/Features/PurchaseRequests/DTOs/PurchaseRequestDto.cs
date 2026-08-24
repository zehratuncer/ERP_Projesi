using ERP.Domain.Enums;

namespace ERP.Application.Features.PurchaseRequests.DTOs;

public class PurchaseRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? RequesterUserId { get; set; }
    public string RequesterUserName { get; set; } = string.Empty;
    public RequestPriority Priority { get; set; }
    public string PriorityName => Priority switch
    {
        RequestPriority.Low => "Düşük",
        RequestPriority.Medium => "Normal",
        RequestPriority.High => "Yüksek",
        RequestPriority.Urgent => "Acil",
        _ => Priority.ToString()
    };
    public RequestStatus Status { get; set; }
    public string StatusName => Status switch
    {
        RequestStatus.Draft => "Taslak",
        RequestStatus.PendingApproval => "Onay Bekliyor",
        RequestStatus.Approved => "Onaylandı",
        RequestStatus.Rejected => "Reddedildi",
        RequestStatus.Completed => "Tamamlandı",
        RequestStatus.Cancelled => "İptal Edildi",
        _ => Status.ToString()
    };
    public decimal TotalEstimatedAmount { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<PurchaseRequestItemDto> Items { get; set; } = new();
}
