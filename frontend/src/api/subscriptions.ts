import api from './client';
import type { SubscriptionResponse, SubscriptionCreateRequest } from '../types';

export const subscriptionsApi = {
  getMine: () =>
    api.get<SubscriptionResponse[]>('/Subscription').then((r) => r.data),

  create: (data: SubscriptionCreateRequest) =>
    api.post<SubscriptionResponse>('/Subscription', data).then((r) => r.data),

  delete: (id: string) =>
    api.delete<boolean>(`/Subscription/${id}`).then((r) => r.data),
};
