import { CartItem, PaymentMethod } from '../../core/models/pos.model';

describe('POS Logic & Calculations', () => {
  function getSubTotal(cart: CartItem[]): number {
    return cart.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
  }

  function getItemDiscountsTotal(cart: CartItem[]): number {
    return cart.reduce((sum, item) => {
      const gross = item.unitPrice * item.quantity;
      return sum + (gross * (item.discountRate || 0)) / 100;
    }, 0);
  }

  function getFinalTotal(cart: CartItem[], generalDiscount: number = 0): number {
    const sub = getSubTotal(cart);
    const discounts = getItemDiscountsTotal(cart) + (generalDiscount || 0);
    return Math.max(0, sub - discounts);
  }

  function getChangeDue(cart: CartItem[], paymentMethod: PaymentMethod, receivedAmount: number, generalDiscount: number = 0): number {
    if (paymentMethod !== PaymentMethod.Cash) return 0;
    const final = getFinalTotal(cart, generalDiscount);
    return Math.max(0, (receivedAmount || 0) - final);
  }

  it('should calculate gross subtotal correctly for multiple items', () => {
    const cart: CartItem[] = [
      {
        product: { id: '1', code: 'KRT-001', name: 'A4 Kağıt', unit: 'Koli', unitPrice: 780, currentStock: 20 },
        quantity: 2,
        unitPrice: 780,
        discountRate: 0
      },
      {
        product: { id: '2', code: 'KRT-003', name: 'Defter', unit: 'Adet', unitPrice: 65, currentStock: 50 },
        quantity: 4,
        unitPrice: 65,
        discountRate: 0
      }
    ];

    // (2 * 780 = 1560) + (4 * 65 = 260) = 1820
    expect(getSubTotal(cart)).toBe(1820);
    expect(getFinalTotal(cart, 0)).toBe(1820);
  });

  it('should calculate item level and general discounts correctly', () => {
    const cart: CartItem[] = [
      {
        product: { id: '1', code: 'KRT-001', name: 'A4 Kağıt', unit: 'Koli', unitPrice: 1000, currentStock: 10 },
        quantity: 1,
        unitPrice: 1000,
        discountRate: 10 // %10 = 100 TL
      }
    ];

    // Gross: 1000, Item Discount: 100, General Discount: 50 => Final: 850
    expect(getItemDiscountsTotal(cart)).toBe(100);
    expect(getFinalTotal(cart, 50)).toBe(850);
  });

  it('should calculate cash change correctly', () => {
    const cart: CartItem[] = [
      {
        product: { id: '1', code: 'KRT-001', name: 'A4 Kağıt', unit: 'Koli', unitPrice: 780, currentStock: 10 },
        quantity: 1,
        unitPrice: 780,
        discountRate: 0
      }
    ];

    // 1000 TL received, Total 780 TL => Change: 220 TL
    const change = getChangeDue(cart, PaymentMethod.Cash, 1000, 0);
    expect(change).toBe(220);
  });

  it('should return 0 change for non-cash payment methods', () => {
    const cart: CartItem[] = [
      {
        product: { id: '1', code: 'KRT-001', name: 'A4 Kağıt', unit: 'Koli', unitPrice: 780, currentStock: 10 },
        quantity: 1,
        unitPrice: 780,
        discountRate: 0
      }
    ];

    const change = getChangeDue(cart, PaymentMethod.CreditCard, 1000, 0);
    expect(change).toBe(0);
  });
});
