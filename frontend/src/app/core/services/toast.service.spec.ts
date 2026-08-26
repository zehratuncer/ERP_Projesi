import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    service = new ToastService();
  });

  it('should add a success toast to signal state', () => {
    service.success('İşlem başarıyla tamamlandı!');
    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('success');
    expect(service.toasts()[0].message).toBe('İşlem başarıyla tamamlandı!');
  });

  it('should add warning and error toasts', () => {
    service.warning('Kritik stok uyarısı!');
    service.error('Sunucu hatası oluştu!');

    expect(service.toasts().length).toBe(2);
    expect(service.toasts()[0].type).toBe('warning');
    expect(service.toasts()[1].type).toBe('error');
  });

  it('should remove toast by id', () => {
    service.info('Bilgilendirme');
    const toastId = service.toasts()[0].id;

    service.remove(toastId);
    expect(service.toasts().length).toBe(0);
  });
});
