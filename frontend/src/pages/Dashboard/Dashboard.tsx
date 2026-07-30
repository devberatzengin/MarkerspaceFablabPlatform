import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { announcementsApi } from '../../api/announcements';
import { equipmentApi } from '../../api/equipment';
import type { AnnouncementResponse, EquipmentResponse, EquipmentStatus, MembershipStatus } from '../../types';

const equipmentStatusLabel: Record<EquipmentStatus, string> = {
  Unknown: 'Bilinmiyor',
  Available: 'Kullanılabilir',
  Reserved: 'Rezerve',
  Rented: 'Kirada',
  Maintenance: 'Bakımda',
};

const equipmentStatusColor: Record<EquipmentStatus, string> = {
  Unknown: 'bg-gray-100 text-gray-800',
  Available: 'bg-green-100 text-green-800',
  Reserved: 'bg-yellow-100 text-yellow-800',
  Rented: 'bg-blue-100 text-blue-800',
  Maintenance: 'bg-red-100 text-red-800',
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
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">
          Hoş geldin, {user?.firstName}!
        </h1>
        <p className="text-gray-500 mt-1">Makerspace Fablab platformuna genel bakış</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-medium text-gray-500">Üyelik</h3>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {membershipLabel[user?.status ?? 'Unknown']}
          </p>
        </div>
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-medium text-gray-500">Ekipman Seviyesi</h3>
          <p className="text-2xl font-bold text-gray-900 mt-1">{user?.equipmentLevel ?? 1} / 10</p>
        </div>
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-medium text-gray-500">Rol</h3>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {isAdmin ? 'Yönetici' : 'Kullanıcı'}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow">
          <div className="px-5 py-4 border-b flex justify-between items-center">
            <h2 className="font-semibold text-gray-900">Son Duyurular</h2>
            <Link to="/announcements" className="text-blue-600 text-sm hover:underline">
              Tümünü Gör
            </Link>
          </div>
          <div className="divide-y">
            {announcements.length === 0 ? (
              <p className="p-5 text-gray-500 text-sm">Henüz duyuru yok.</p>
            ) : (
              announcements.map((a) => (
                <div key={a.id} className="px-5 py-3">
                  <h3 className="font-medium text-gray-900 text-sm">{a.title}</h3>
                  <p className="text-gray-500 text-xs mt-1">
                    {a.createdByName} - {new Date(a.createdAt).toLocaleString('tr-TR')}
                  </p>
                </div>
              ))
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow">
          <div className="px-5 py-4 border-b flex justify-between items-center">
            <h2 className="font-semibold text-gray-900">Ekipmanlar</h2>
            <Link to="/equipment" className="text-blue-600 text-sm hover:underline">
              Tümünü Gör
            </Link>
          </div>
          <div className="divide-y">
            {equipment.length === 0 ? (
              <p className="p-5 text-gray-500 text-sm">Henüz ekipman yok.</p>
            ) : (
              equipment.map((eq) => (
                <div key={eq.id} className="px-5 py-3 flex justify-between items-center">
                  <div>
                    <h3 className="font-medium text-gray-900 text-sm">{eq.name}</h3>
                    <p className="text-gray-500 text-xs mt-0.5">{eq.description}</p>
                  </div>
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${equipmentStatusColor[eq.status]}`}>
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
