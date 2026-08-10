import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { announcementsApi } from '../../api/announcements';
import { equipmentApi } from '../../api/equipment';
import ActivityCalendar from '../../components/ActivityCalendar';
import type { AnnouncementResponse, EquipmentResponse, EquipmentStatus, MembershipStatus } from '../../types';

const equipmentStatusLabel: Record<EquipmentStatus, string> = {
  Unknown: 'Bilinmiyor',
  Available: 'Kullanılabilir',
  Reserved: 'Rezerve',
  Rented: 'Kirada',
  Maintenance: 'Bakımda',
};

const equipmentStatusBadge: Record<EquipmentStatus, string> = {
  Unknown: 'badge badge-inactive',
  Available: 'badge badge-available',
  Reserved: 'badge badge-reserved',
  Rented: 'badge badge-info',
  Maintenance: 'badge badge-maintenance',
};

const membershipLabel: Record<MembershipStatus, string> = {
  Unknown: 'Bilinmiyor',
  Free: 'Ücretsiz',
  Bronze: 'Bronz',
  Silver: 'Gümüş',
  Gold: 'Altın',
  Professional: 'Profesyonel',
};

export default function Dashboard() {
  const { user, isAdmin } = useAuth();
  const [announcements, setAnnouncements] = useState<AnnouncementResponse[]>([]);
  const [equipment, setEquipment] = useState<EquipmentResponse[]>([]);

  useEffect(() => {
    announcementsApi
      .getAll({ page: 1, pageSize: 5, status: 'Published' })
      .then((res) => setAnnouncements(res.items))
      .catch(() => {});
    equipmentApi
      .getAll({ page: 1, pageSize: 5 })
      .then((res) => setEquipment(res.items))
      .catch(() => {});
  }, []);

  return (
    <div>
      <h1 className="page-title">Hoş geldin, {user?.firstName}!</h1>
      <p className="page-subtitle">Makerspace Fablab platformuna genel bakış</p>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <div className="card-compact">
          <h3 className="text-sm font-medium text-gray-500">Üyelik</h3>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {membershipLabel[user?.status ?? 'Unknown']}
          </p>
        </div>
        <div className="card-compact">
          <h3 className="text-sm font-medium text-gray-500">Ekipman Seviyesi</h3>
          <p className="text-2xl font-bold text-gray-900 mt-1">{user?.equipmentLevel ?? 1} / 10</p>
        </div>
        <div className="card-compact">
          <h3 className="text-sm font-medium text-gray-500">Rol</h3>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {isAdmin ? 'Yönetici' : 'Kullanıcı'}
          </p>
        </div>
      </div>

      <div className="mb-8">
        <ActivityCalendar />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="card">
          <div className="flex justify-between items-center pb-4 border-b border-gray-200">
            <h2 className="card-title">Son Duyurular</h2>
            <Link to="/announcements" className="text-blue-600 text-sm hover:underline">
              Tümünü Gör
            </Link>
          </div>
          <div className="divide-y -mx-6 mt-4">
            {announcements.length === 0 ? (
              <p className="px-6 py-3 text-gray-500 text-sm">Henüz duyuru yok.</p>
            ) : (
              announcements.map((a) => (
                <div key={a.id} className="px-6 py-3">
                  <h3 className="font-medium text-gray-900 text-sm">{a.title}</h3>
                  <p className="text-gray-500 text-xs mt-1">
                    {a.createdByName} - {new Date(a.createdAt).toLocaleString('tr-TR')}
                  </p>
                </div>
              ))
            )}
          </div>
        </div>

        <div className="card">
          <div className="flex justify-between items-center pb-4 border-b border-gray-200">
            <h2 className="card-title">Ekipmanlar</h2>
            <Link to="/equipment" className="text-blue-600 text-sm hover:underline">
              Tümünü Gör
            </Link>
          </div>
          <div className="divide-y -mx-6 mt-4">
            {equipment.length === 0 ? (
              <p className="px-6 py-3 text-gray-500 text-sm">Henüz ekipman yok.</p>
            ) : (
              equipment.map((eq) => (
                <div key={eq.id} className="px-6 py-3 flex justify-between items-center">
                  <div>
                    <h3 className="font-medium text-gray-900 text-sm">{eq.name}</h3>
                    <p className="text-gray-500 text-xs mt-0.5">{eq.description}</p>
                  </div>
                  <span className={equipmentStatusBadge[eq.status]}>
                    {equipmentStatusLabel[eq.status]}
                  </span>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
