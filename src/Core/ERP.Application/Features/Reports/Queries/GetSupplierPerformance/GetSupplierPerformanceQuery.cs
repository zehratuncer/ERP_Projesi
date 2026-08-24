using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Reports.DTOs;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Reports.Queries.GetSupplierPerformance;

public record GetSupplierPerformanceQuery : IRequest<ApiResponse<SupplierPerformanceDto>>;

public class GetSupplierPerformanceQueryHandler : IRequestHandler<GetSupplierPerformanceQuery, ApiResponse<SupplierPerformanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSupplierPerformanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<SupplierPerformanceDto>> Handle(GetSupplierPerformanceQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await _context.Suppliers
            .Include(s => s.Products)
            .Where(s => !s.IsDeleted && s.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Satın Alma Talepleri ve Mal Kabul Verileri
        var purchaseRequests = await _context.PurchaseRequests
            .Include(pr => pr.Items)
            .Where(pr => !pr.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var supplierPerformanceList = new List<SupplierPerformanceItemDto>();

        foreach (var supplier in suppliers)
        {
            var supplierProductIds = supplier.Products.Select(p => p.Id).ToHashSet();
            int productCount = supplierProductIds.Count;

            // Bu tedarikçiye ait ürünleri içeren talepler
            var relatedRequests = purchaseRequests
                .Where(pr => pr.Items.Any(i => supplierProductIds.Contains(i.ProductId)))
                .ToList();

            var completedRequests = relatedRequests.Where(pr => pr.Status == RequestStatus.Completed).ToList();
            var pendingRequests = relatedRequests.Where(pr => pr.Status == RequestStatus.PendingApproval || pr.Status == RequestStatus.Approved).ToList();

            int totalRequests = relatedRequests.Count;
            int completedCount = completedRequests.Count;
            int pendingCount = pendingRequests.Count;

            decimal totalAmount = completedRequests.Sum(pr => pr.TotalEstimatedAmount);

            // Teslimat Süresi Hesaplama (CreatedDate ile UpdatedDate arası gün ortalaması)
            double avgDeliveryDays = 3.0; // Varsayılan hızlı tedarik (ortalama 3 gün)
            if (completedRequests.Any())
            {
                var daysList = completedRequests
                    .Select(pr => Math.Max(1.0, (pr.UpdatedDate.HasValue ? (pr.UpdatedDate.Value - pr.CreatedDate).TotalDays : 2.5)))
                    .ToList();
                avgDeliveryDays = Math.Round(daysList.Average(), 1);
            }

            // Karşılama Oranı %
            double fulfillmentRate = totalRequests > 0 
                ? Math.Round(((double)completedCount / totalRequests) * 100.0, 1) 
                : 95.0; // Yeni tedarikçiler için başlangıç 95

            // Güvenilirlik Puanı (100 üzerinden): Formül = FulfillmentRate * 0.6 + (10 - Min(10, avgDays)) * 4
            double timeScore = Math.Max(0, (10 - Math.Min(10, avgDeliveryDays))) * 4.0;
            double reliability = Math.Round((fulfillmentRate * 0.6) + timeScore, 1);
            if (reliability > 100) reliability = 100.0;

            string grade = reliability >= 90 ? "A (Mükemmel)" : (reliability >= 75 ? "B (İyi)" : "C (Geliştirilmeli)");

            supplierPerformanceList.Add(new SupplierPerformanceItemDto
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                SuppliedProductCount = productCount,
                CompletedRequestCount = completedCount,
                PendingRequestCount = pendingCount,
                TotalSuppliedAmount = totalAmount,
                AverageDeliveryDays = avgDeliveryDays,
                FulfillmentRate = fulfillmentRate,
                ReliabilityScore = reliability,
                PerformanceGrade = grade
            });
        }

        supplierPerformanceList = supplierPerformanceList
            .OrderByDescending(s => s.ReliabilityScore)
            .ThenByDescending(s => s.TotalSuppliedAmount)
            .ToList();

        double avgFulfillment = supplierPerformanceList.Any() 
            ? Math.Round(supplierPerformanceList.Average(s => s.FulfillmentRate), 1) 
            : 0;

        decimal totalProcured = supplierPerformanceList.Sum(s => s.TotalSuppliedAmount);

        var result = new SupplierPerformanceDto
        {
            TotalSuppliers = supplierPerformanceList.Count,
            AverageOverallFulfillmentRate = avgFulfillment,
            TotalProcuredVolume = totalProcured,
            Suppliers = supplierPerformanceList
        };

        return ApiResponse<SupplierPerformanceDto>.Success(result, "Tedarikçi teslimat, karşılama ve güvenilirlik performans analizi başarıyla tamamlandı.");
    }
}
