import api from './client';
import type {
  EquipmentResponse,
  EquipmentCreateRequest,
  EquipmentUpdateRequest,
  EquipmentRentalResponse,
  ReserveAheadRequest,
  PagedResponse,
  EquipmentStatus,
  EquipmentType,
  EquipmentPlacementType,
} from '../types';

interface EquipmentListParams {
  page?: number;
  pageSize?: number;
  status?: EquipmentStatus;
  type?: EquipmentType;
  placementType?: EquipmentPlacementType;
  search?: string;
}

export const equipmentApi = {
  getAll: (params: EquipmentListParams = {}) =>
    api.get<PagedResponse<EquipmentResponse>>('/Equipment', { params }).then((r) => r.data),

  getById: (id: string) =>
    api.get<EquipmentResponse>(`Equipment/${id}`).then((r) => r.data),

  create: (data: EquipmentCreateRequest) =>
    api.post<EquipmentResponse>('/Equipment', data).then((r) => r.data),

  update: (data: EquipmentUpdateRequest) =>
    api.put<EquipmentResponse>('/Equipment', data).then((r) => r.data),

  delete: (id: string) =>
    api.delete<EquipmentResponse>('/Equipment', { params: { id } }).then((r) => r.data),

  rent: (id: string, span: string) =>
    api.patch<EquipmentResponse>(`/Equipment/${id}/rent`, null, { params: { span } }).then((r) => r.data),

  reserve: (id: string, span: string) =>
    api.patch<EquipmentResponse>(`/Equipment/${id}/reserve`, null, { params: { span } }).then((r) => r.data),

  // İleri tarihli rezervasyon. Mevcut /reserve ucunu bozmamak için ayrı bir uç kullanır;
  // backend tarafında iki uç birleştirilirse burada sadece URL değişecek.
  reserveAhead: (id: string, data: ReserveAheadRequest) =>
    api.patch<EquipmentResponse>(`/Equipment/${id}/reserve-ahead`, data).then((r) => r.data),

  release: (id: string) =>
    api.patch<EquipmentResponse>(`/Equipment/${id}/release`).then((r) => r.data),

  maintenance: (id: string) =>
    api.patch<EquipmentResponse>(`/Equipment/${id}/maintenance`).then((r) => r.data),

  unmaintenance: (id: string) =>
    api.patch<EquipmentResponse>(`/Equipment/${id}/unmaintenance`).then((r) => r.data),

  myEquipments: (params: { page?: number; pageSize?: number; includePast?: boolean } = {}) =>
    api.get<PagedResponse<EquipmentRentalResponse>>('/Equipment/my-equipments', { params }).then((r) => r.data),
};
