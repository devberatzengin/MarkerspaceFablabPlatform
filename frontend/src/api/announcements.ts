import api from './client';
import type {
  AnnouncementResponse,
  AnnouncementCreateRequest,
  AnnouncementUpdateRequest,
  PagedResponse,
  ContentStatus,
} from '../types';

interface AnnouncementListParams {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  status?: ContentStatus;
  search?: string;
}

export const announcementsApi = {
  getAll: (params: AnnouncementListParams = {}) =>
    api.get<PagedResponse<AnnouncementResponse>>('/Announcement', { params }).then((r) => r.data),

  getById: (id: string) =>
    api.get<AnnouncementResponse>(`/Announcement/${id}`).then((r) => r.data),

  create: (data: AnnouncementCreateRequest) =>
    api.post<AnnouncementResponse>('/Announcement', data).then((r) => r.data),

  update: (id: string, data: AnnouncementUpdateRequest) =>
    api.put<AnnouncementResponse>(`/Announcement/${id}`, data).then((r) => r.data),

  publish: (id: string) =>
    api.patch<AnnouncementResponse>(`/Announcement/${id}/publish`).then((r) => r.data),

  unpublish: (id: string) =>
    api.patch<AnnouncementResponse>(`/Announcement/${id}/unpublish`).then((r) => r.data),

  archive: (id: string) =>
    api.patch<boolean>(`/Announcement/${id}/archive`).then((r) => r.data),
};
