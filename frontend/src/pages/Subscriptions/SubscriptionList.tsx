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
        <h1 className="page-title">Aboneliklerim</h1>
        <p className="page-subtitle mb-0">
          Takip ettiğin kategoride yeni duyuru yayınlandığında bildirim alırsın.
        </p>
      </div>

      {error && <div className="error-banner mb-4">{error}</div>}

      <div className="card mb-6">
        <h2 className="card-title">Yeni Takip</h2>
        <form onSubmit={handleCreate} className="grid gap-3 md:grid-cols-4">
          <select
            value={targetType}
            onChange={(e) => setTargetType(e.target.value as SubscriptionTargetType)}
            className="select"
          >
            <option value="Category">Kategori</option>
            <option value="Equipment">Ekipman</option>
          </select>

          <select
            value={targetId}
            onChange={(e) => setTargetId(e.target.value)}
            className="select"
          >
            <option value="">Seçiniz...</option>
            {targetOptions.map((o) => (
              <option key={o.id} value={o.id}>{o.name}</option>
            ))}
          </select>

          <select
            value={channel}
            onChange={(e) => setChannel(e.target.value as NotificationChannelType)}
            className="select"
          >
            <option value="InApp">Uygulama İçi</option>
            <option value="Email">E-posta</option>
          </select>

          <button type="submit" className="btn-primary">
            Takip Et
          </button>
        </form>
        {channel === 'Email' && (
          <p className="info-banner mt-3">
            E-posta kanalı henüz hazır değil (SMTP adapter yazılmadı). Test için Uygulama İçi seç.
          </p>
        )}
      </div>

      <div className="card p-0 overflow-hidden">
        <table className="table">
          <thead>
            <tr>
              <th>Hedef</th>
              <th>Tip</th>
              <th>Kanal</th>
              <th>Durum</th>
              <th>İşlem</th>
            </tr>
          </thead>
          <tbody>
            {subscriptions.map((s) => (
              <tr key={s.id}>
                <td className="font-medium text-gray-900">
                  {s.categoryName ?? s.equipmentName ?? '-'}
                </td>
                <td className="text-gray-500">
                  {s.targetType === 'Category' ? 'Kategori' : 'Ekipman'}
                </td>
                <td className="text-gray-500">{channelLabel[s.channel] ?? s.channel}</td>
                <td>
                  <span className={`badge ${s.isActive ? 'badge-active' : 'badge-inactive'}`}>
                    {s.isActive ? 'Takipte' : 'Bırakıldı'}
                  </span>
                </td>
                <td>
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
