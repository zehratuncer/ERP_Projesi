using ERP.Domain.Enums;

namespace ERP.Application.Features.PurchaseRequests.DTOs;

public class ApprovalHistoryDto
{
    public Guid Id { get; set; }
    public Guid PurchaseRequestId { get; set; }
    public Guid? ApproverUserId { get; set; }
    public string ApproverUserName { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public ApprovalAction Action { get; set; }
    public string ActionName => Action switch
    {
        ApprovalAction.Approved => "Onaylandı",
        ApprovalAction.Rejected => "Reddedildi",
        ApprovalAction.Revised => "Revizyon İstendi",
        _ => Action.ToString()
    };
    public string? Comment { get; set; }
    public DateTime ActionDate { get; set; }
}
