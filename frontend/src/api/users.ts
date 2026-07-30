import api from './client';
import type { UserResponse, UserUpdateRequest, ChangePasswordRequest } from '../types';

export const usersApi = {
  getAll: () =>
    api.get<UserResponse[]>('/Users').then((r) => r.data),

  getMe: () =>
    api.get<UserResponse>('/Users/me').then((r) => r.data),

  getById: (id: string) =>
    api.get<UserResponse>(`/Users/${id}`).then((r) => r.data),

  update: (id: string, data: UserUpdateRequest) =>
    api.put<UserResponse>(`/Users/${id}`, data).then((r) => r.data),

  deactivate: (id: string) =>
    api.patch(`/Users/${id}/deactivate`),

  activate: (id: string) =>
    api.patch(`/Users/${id}/activate`),

  delete: (id: string) =>
    api.delete(`/Users/${id}`),

  changePassword: (data: ChangePasswordRequest) =>
    api.post('/Users/me/change-password', data),

  addBalance: (balance: number) =>
    api.post<UserResponse>('/Users/me/add-balance', null, { params: { balance } }).then((r) => r.data),
};
