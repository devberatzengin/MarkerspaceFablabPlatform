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
      return 'bg-green-100 text-green-800';
    case 'Pending':
      return 'bg-amber-100 text-amber-800';
    case 'Failed':
      return 'bg-red-100 text-red-800';
    case 'Refunded':
      return 'bg-blue-100 text-blue-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
};

/** .NET ProblemDetails'ten okunabilir mesaj çıkarır. */
export const errorMessage = (err: unknown, fallback = 'Bir hata oluştu.') => {
  const detail = (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail;
  return detail || fallback;
};
