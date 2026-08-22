namespace ERP.Domain.Enums;

public enum TransactionType
{
    In = 1,          // Stok Girişi (Satın alma, üretimden giriş vb.)
    Out = 2,         // Stok Çıkışı (Satış, üretime sevk, sarfiyat vb.)
    Adjustment = 3   // Stok Sayım Düzeltmesi (Fiziki sayım farkları vb.)
}
