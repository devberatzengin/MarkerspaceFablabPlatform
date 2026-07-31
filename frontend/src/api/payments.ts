import api from './client';
import type {
  PagedResponse,
  PaymentCreateRequest,
  PaymentMethod,
  PaymentResponse,
  PaymentStatus,
  PendingPaymentResponse,
} from '../types';

interface PaymentListParams {
  page?: number;
  pageSize?: number;
  status?: PaymentStatus;
  paymentMethod?: PaymentMethod;
  search?: string;
}

export const paymentsApi = {
  // Admin: tüm ödemeler
  getAll: (params: PaymentListParams = {}) =>
    api.get<PagedResponse<PaymentResponse>>('/Payment', { params }).then((r) => r.data),

  myPayments: (params: PaymentListParams = {}) =>
    api.get<PagedResponse<PaymentResponse>>('/Payment/my-payments', { params }).then((r) => r.data),

  // Ödenmemiş kiralamalar, tutarları hesaplanmış halde
  pending: (params: PaymentListParams = {}) =>
    api.get<PagedResponse<PendingPaymentResponse>>('/Payment/pending', { params }).then((r) => r.data),

  // Ödeme ekranındaki tutarı getirir, hiçbir şey kaydetmez
  preview: (equipmentRentalId: string) =>
    api.get<PendingPaymentResponse>(`/Payment/preview/${equipmentRentalId}`).then((r) => r.data),

  getById: (id: string) =>
    api.get<PaymentResponse>(`/Payment/${id}`).then((r) => r.data),

  create: (data: PaymentCreateRequest) =>
    api.post<PaymentResponse>('/Payment', data).then((r) => r.data),
};
