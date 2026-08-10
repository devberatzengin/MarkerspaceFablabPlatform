import { useEffect, useState } from 'react';
import { notificationsApi } from '../../api/notifications';
import type { NotificationResponse, NotificationType, PagedResponse } from '../../types';

const typeLabel: Record<NotificationType, string> = {
  AnnouncementPublished: 'Yeni Duyuru',
  EquipmentMaintenance: 'Bakım',
  EquipmentAvailable: 'Ekipman Müsait',
  RentalEndsSoon: 'Süre Doluyor',
};

const typeBadge: Record<NotificationType, string> = {
  AnnouncementPublished: 'bg-blue-100 text-blue-800',
  EquipmentMaintenance: 'bg-yellow-100 text-yellow-800',
  EquipmentAvailable: 'bg-green-100 text-green-800',
  RentalEndsSoon: 'bg-red-100 text-red-800',
};

export default function NotificationList() {
  const [data, setData] = useState<PagedResponse<NotificationResponse> | null>(null);
  const [onlyUnread, setOnlyUnread] = useState(false);
  const [page, setPage] = useState(1);
  const [error, setError] = useState('');

  const fetchData = async () => {
    try {
      const result = await notificationsApi.getMine({ page, pageSize: 20, onlyUnread });
      setData(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Bildirimler yüklenemedi');
    }
  };

  useEffect(() => {
    fetchData();
  }, [page, onlyUnread]);

  const handleMarkAsRead = async (id: string) => {
    try {
      await notificationsApi.markAsRead(id);
      fetchData();
    } catch {}
  };

  const items = data?.items ?? [];

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="page-title mb-0">Bildirimler</h1>
        <button
          onClick={() => { setOnlyUnread((v) => !v); setPage(1); }}
          className="btn-secondary"
        >
          {onlyUnread ? 'Tümünü Göster' : 'Sadece Okunmamışlar'}
        </button>
      </div>

      {error && <div className="error-banner mb-4">{error}</div>}

      <div className="card p-0 divide-y divide-gray-100">
        {items.map((n) => (
          <div key={n.id} className={`px-5 py-4 flex items-start justify-between gap-4 ${n.isRead ? '' : 'bg-blue-50/40'}`}>
            <div className="min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <span className={`badge ${typeBadge[n.type] ?? 'bg-gray-100 text-gray-800'}`}>
                  {typeLabel[n.type] ?? n.type}
                </span>
                {!n.isRead && <span className="w-2 h-2 rounded-full bg-blue-600" />}
                <span className="text-xs text-gray-400">{new Date(n.createdAt).toLocaleString('tr-TR')}</span>
              </div>
              <p className="font-medium text-gray-900">{n.title}</p>
              <p className="text-sm text-gray-600 mt-0.5">{n.message}</p>
            </div>
            {!n.isRead && (
              <button
                onClick={() => handleMarkAsRead(n.id)}
                className="shrink-0 text-blue-600 hover:underline text-xs"
              >
                Okundu işaretle
              </button>
            )}
          </div>
        ))}

        {items.length === 0 && (
          <div className="px-5 py-10 text-center text-gray-500">
            {onlyUnread ? 'Okunmamış bildirim yok.' : 'Henüz bildiriminiz yok.'}
          </div>
        )}
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex justify-center items-center gap-3 mt-4">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="btn-secondary disabled:opacity-40"
          >
            Önceki
          </button>
          <span className="text-sm text-gray-600">{data.page} / {data.totalPages}</span>
          <button
            onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
            disabled={page >= data.totalPages}
            className="btn-secondary disabled:opacity-40"
          >
            Sonraki
          </button>
        </div>
      )}
    </div>
  );
}
