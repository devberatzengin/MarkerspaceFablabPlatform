import api from './client';
import type { CategoryResponse, CategoryCreateRequest, CategoryUpdateRequest } from '../types';

export const categoriesApi = {
  getAll: (includeUnactivated = false) =>
    api.get<CategoryResponse[]>('/Category', { params: { includeUnactivated } }).then((r) => r.data),
  
  getById: (id: string, includeUnactivated = false) =>
    api.get<CategoryResponse>(`/Category/${id}`, { params: { includeUnactivated } }).then((r) => r.data),

  create: (data: CategoryCreateRequest) =>
    api.post<CategoryResponse>('/Category', data).then((r) => r.data),

  update: (data: CategoryUpdateRequest) =>
    api.put<CategoryResponse>('/Category', data).then((r) => r.data),

  deactivate: (id: string) =>
    api.patch<CategoryResponse>(`/Category/${id}/deactivate`).then((r) => r.data),

  activate: (id: string) =>
      api.patch(`/Category/${id}/activate`).then((r) => r.data),
  
  delete: (id: string) =>
    api.delete<boolean>('/Category', { params: { categoryId: id } }).then((r) => r.data),
};
