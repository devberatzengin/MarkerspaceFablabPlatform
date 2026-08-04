import api from './client';
import type { NotificationResponse, PagedResponse } from '../types';

interface NotificationListParams {
  page?: number;
  pageSize?: number;
  onlyUnread?: boolean;
}

export const notificationsApi = {
  getMine: (params: NotificationListParams = {}) =>
    api.get<PagedResponse<NotificationResponse>>('/Notification', { params }).then((r) => r.data),

  getUnreadCount: () =>
    api.get<number>('/Notification/unread-count').then((r) => r.data),

  markAsRead: (id: string) =>
    api.patch<NotificationResponse>(`/Notification/${id}/read`).then((r) => r.data),
};
