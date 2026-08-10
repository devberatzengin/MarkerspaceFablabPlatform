import type { PaymentStatus } from '../types';

export const formatMoney = (amount: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(amount);

export const paymentStatusLabel = (status: PaymentStatus) => {
  switch (status) {
    case 'Pending':
      return 'Bekliyor';
    case 'Paid':
      return 'Ödendi';
    case 'Failed':
      return 'Başarısız';
    case 'Refunded':
      return 'İade Edildi';
    case 'Cancelled':
      return 'İptal';
    default:
      return status;
  }
};

export const paymentStatusBadge = (status: PaymentStatus) => {
  switch (status) {
    case 'Paid':
      return 'badge-paid';
    case 'Pending':
      return 'badge-pending';
    case 'Failed':
      return 'badge-failed';
    case 'Refunded':
      return 'badge-info';
    default:
      return 'badge-cancelled';
  }
};

/** .NET ProblemDetails'ten okunabilir mesaj çıkarır. */
export const errorMessage = (err: unknown, fallback = 'Bir hata oluştu.') => {
  const detail = (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail;
  return detail || fallback;
};
