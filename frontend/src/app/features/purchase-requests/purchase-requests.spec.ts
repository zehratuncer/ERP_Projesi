import { RequestStatus, RequestPriority } from '../../core/models/purchase-request.model';

describe('Purchase Requests Logic & Calculations', () => {
  interface FormItem {
    productId: string;
    requestedQuantity: number;
    estimatedUnitPrice: number;
  }

  function getFormEstimatedTotal(items: FormItem[]): number {
    return items.reduce((sum, i) => sum + (Number(i.requestedQuantity || 0) * Number(i.estimatedUnitPrice || 0)), 0);
  }

  it('should calculate estimated total for multiple request rows', () => {
    const items: FormItem[] = [
      { productId: '1', requestedQuantity: 10, estimatedUnitPrice: 750 }, // 7500
      { productId: '2', requestedQuantity: 20, estimatedUnitPrice: 60 }    // 1200
    ];

    expect(getFormEstimatedTotal(items)).toBe(8700);
  });

  it('should return 0 when items list is empty or quantities are 0', () => {
    expect(getFormEstimatedTotal([])).toBe(0);
    expect(getFormEstimatedTotal([{ productId: '1', requestedQuantity: 0, estimatedUnitPrice: 500 }])).toBe(0);
  });

  it('should have correct enum mappings for status and priority', () => {
    expect(RequestStatus.Draft).toBe(1);
    expect(RequestStatus.PendingApproval).toBe(2);
    expect(RequestStatus.Approved).toBe(3);
    expect(RequestStatus.Rejected).toBe(4);
    expect(RequestStatus.Completed).toBe(5);

    expect(RequestPriority.Low).toBe(1);
    expect(RequestPriority.Medium).toBe(2);
    expect(RequestPriority.High).toBe(3);
    expect(RequestPriority.Urgent).toBe(4);
  });
});
