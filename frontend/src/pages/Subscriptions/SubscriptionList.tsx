import { useEffect, useState } from 'react';
import { subscriptionsApi } from '../../api/subscriptions';
import { categoriesApi } from '../../api/categories';
import { equipmentApi } from '../../api/equipment';
import type {
  SubscriptionResponse,
  SubscriptionTargetType,
  NotificationChannelType,
  CategoryResponse,
  EquipmentResponse,
} from '../../types';

const channelLabel: Record<NotificationChannelType, string> = {
  InApp: 'Uygulama İçi',
  Email: 'E-posta',
};

export default function SubscriptionList() {
  const [subscriptions, setSubscriptions] = useState<SubscriptionResponse[]>([]);
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);
  const [targetType, setTargetType] = useState<SubscriptionTargetType>('Category');
  const [targetId, setTargetId] = useState('');
  const [channel, setChannel] = useState<NotificationChannelType>('InApp');
  const [error, setError] = useState('');

  const fetchData = async () => {
    try {
      const data = await subscriptionsApi.getMine();
      setSubscriptions(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Abonelikler yüklenemedi');
    }
  };

  useEffect(() => {
    fetchData();

    categoriesApi.getAll().then(setCategories).catch(() => {});
    equipmentApi.getAll({ pageSize: 100 }).then((r) => setEquipments(r.items)).catch(() => {});
  }, []);

  useEffect(() => {
    setTargetId('');
  }, [targetType]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!targetId) {
      setError('Lütfen bir hedef seçin.');
      return;
    }

    try {
      await subscriptionsApi.create({
        targetType,
        categoryId: targetType === 'Category' ? targetId : undefined,
        equipmentId: targetType === 'Equipment' ? targetId : undefined,
        channel,
      });
      setTargetId('');
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.detail || err.response?.data?.message || 'Abonelik oluşturulamadı');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Bu takibi bırakmak istediğinize emin misiniz?')) return;
    try {
      await subscriptionsApi.delete(id);
      fetchData();
    } catch {}
  };

  const targetOptions =
    targetType === 'Category'
      ? categories.map((c) => ({ id: c.id, name: c.name }))
      : equipments.map((e) => ({ id: e.id, name: e.name }));

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Aboneliklerim</h1>
        <p className="text-sm text-gray-500 mt-1">
          Takip ettiğin kategoride yeni duyuru yayınlandığında bildirim alırsın.
        </p>
      </div>

      {error && <div className="bg-red-50 text-red-600 p-3 rounded mb-4 text-sm">{error}</div>}

      <div className="bg-white rounded-lg shadow p-5 mb-6">
        <h2 className="font-semibold text-gray-900 mb-4">Yeni Takip</h2>
        <form onSubmit={handleCreate} className="grid gap-3 md:grid-cols-4">
          <select
            value={targetType}
            onChange={(e) => setTargetType(e.target.value as SubscriptionTargetType)}
            className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="Category">Kategori</option>
            <option value="Equipment">Ekipman</option>
          </select>

          <select
            value={targetId}
            onChange={(e) => setTargetId(e.target.value)}
            className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">Seçiniz...</option>
            {targetOptions.map((o) => (
              <option key={o.id} value={o.id}>{o.name}</option>
            ))}
          </select>

          <select
            value={channel}
            onChange={(e) => setChannel(e.target.value as NotificationChannelType)}
            className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="InApp">Uygulama İçi</option>
            <option value="Email">E-posta</option>
          </select>

          <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm">
            Takip Et
          </button>
        </form>
        {channel === 'Email' && (
          <p className="text-xs text-yellow-700 bg-yellow-50 rounded px-3 py-2 mt-3">
            E-posta kanalı henüz hazır değil (SMTP adapter yazılmadı). Test için Uygulama İçi seç.
          </p>
        )}
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">Hedef</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">Tip</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">Kanal</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">Durum</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">İşlem</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {subscriptions.map((s) => (
              <tr key={s.id} className="hover:bg-gray-50">
                <td className="px-5 py-3 font-medium text-gray-900">
                  {s.categoryName ?? s.equipmentName ?? '-'}
                </td>
                <td className="px-5 py-3 text-gray-500">
                  {s.targetType === 'Category' ? 'Kategori' : 'Ekipman'}
                </td>
                <td className="px-5 py-3 text-gray-500">{channelLabel[s.channel] ?? s.channel}</td>
                <td className="px-5 py-3">
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${s.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
                    {s.isActive ? 'Takipte' : 'Bırakıldı'}
                  </span>
                </td>
                <td className="px-5 py-3">
                  {s.isActive && (
                    <button onClick={() => handleDelete(s.id)} className="text-red-600 hover:underline text-xs">
                      Takibi Bırak
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {subscriptions.length === 0 && (
              <tr>
                <td colSpan={5} className="px-5 py-8 text-center text-gray-500">Henüz bir takibiniz yok.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
