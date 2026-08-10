import { useEffect, useState } from 'react';
import { usersApi } from '../../api/users';
import DetailModal, { DetailRow } from '../../components/DetailModal';
import type { UserResponse, UserType, MembershipStatus } from '../../types';

const membershipLabel: Record<MembershipStatus, string> = {
  Unknown: 'Bilinmiyor',
  Free: 'Ücretsiz',
  Bronze: 'Bronz',
  Silver: 'Gümüş',
  Gold: 'Altın',
  Professional: 'Profesyonel',
};

const roleLabel: Record<UserType, string> = {
  Unknown: 'Bilinmiyor',
  Admin: 'Yönetici',
  User: 'Kullanıcı',
  Staff: 'Personel',
};

export default function UserList() {
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [showInactive, setShowInactive] = useState(false);
  const [detailItem, setDetailItem] = useState<UserResponse | null>(null);

  const openDetail = async (id: string) => {
    try {
      const item = await usersApi.getById(id);
      setDetailItem(item);
    } catch {}
  };

  const fetchData = async () => {
    try {
      const data = await usersApi.getAll();
      setUsers(data);
    } catch {}
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleActivate = async (id: string) => {
    try {
      await usersApi.activate(id);
      fetchData();
    } catch {}
  };

  const handleDeactivate = async (id: string) => {
    try {
      await usersApi.deactivate(id);
      fetchData();
    } catch {}
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Bu kullanıcıyı silmek istediğinize emin misiniz?')) return;
    try {
      await usersApi.delete(id);
      fetchData();
    } catch {}
  };

  return (
    <div>
      {detailItem && (
        <DetailModal title="Kullanıcı Detayı" onClose={() => setDetailItem(null)}>
          <DetailRow label="ID" value={detailItem.id} />
          <DetailRow label="Kullanıcı Adı" value={detailItem.username} />
          <DetailRow label="Ad" value={detailItem.firstName} />
          <DetailRow label="Soyad" value={detailItem.lastName} />
          <DetailRow label="E-posta" value={detailItem.email} />
          <DetailRow label="Telefon" value={detailItem.phoneNumber} />
          <DetailRow label="Rol" value={roleLabel[detailItem.type]} />
          <DetailRow label="Ekipman Seviyesi" value={detailItem.equipmentLevel} />
          <DetailRow label="Üyelik" value={membershipLabel[detailItem.status]} />
          <DetailRow label="Durum" value={detailItem.isActive ? 'Aktif' : 'Pasif'} badge={detailItem.isActive ? 'badge-active' : 'badge-inactive'} />
          <DetailRow label="Kayıt Tarihi" value={new Date(detailItem.createdAt).toLocaleString('tr-TR')} />
        </DetailModal>
      )}

      <div className="flex justify-between items-center mb-6">
        <h1 className="page-title mb-0">Kullanıcılar</h1>
        <button
          onClick={() => setShowInactive((v) => !v)}
          className="btn-secondary"
        >
          {showInactive ? 'Pasifleri Gizle' : 'Pasifleri Göster'}
        </button>
      </div>

      <div className="card p-0 overflow-hidden">
        <table className="table">
          <thead>
            <tr>
              <th>Ad Soyad</th>
              <th className="hidden md:table-cell">E-posta</th>
              <th className="hidden md:table-cell">Kullanıcı Adı</th>
              <th>Rol</th>
              <th className="hidden md:table-cell">Üyelik</th>
              <th>Durum</th>
              <th>İşlem</th>
            </tr>
          </thead>
          <tbody>
            {users.filter((u) => showInactive || u.isActive).map((u) => (
              <tr key={u.id} className="cursor-pointer" onClick={() => openDetail(u.id)}>
                <td className="font-medium text-gray-900">{u.firstName} {u.lastName}</td>
                <td className="hidden md:table-cell text-gray-500">{u.email}</td>
                <td className="hidden md:table-cell text-gray-500">{u.username}</td>
                <td className="text-gray-500">{roleLabel[u.type]}</td>
                <td className="hidden md:table-cell text-gray-500">{membershipLabel[u.status]}</td>
                <td>
                  <span className={`badge ${u.isActive ? 'badge-active' : 'badge-inactive'}`}>
                    {u.isActive ? 'Aktif' : 'Pasif'}
                  </span>
                </td>
                <td onClick={(e) => e.stopPropagation()}>
                  <div className="flex gap-2">
                    {u.isActive ? (
                      <button onClick={() => handleDeactivate(u.id)} className="text-yellow-600 hover:underline text-xs">Pasifleştir</button>
                    ) : (
                      <button onClick={() => handleActivate(u.id)} className="text-green-600 hover:underline text-xs">Aktifleştir</button>
                    )}
                    <button onClick={() => handleDelete(u.id)} className="btn-danger">Sil</button>
                  </div>
                </td>
              </tr>
            ))}
            {users.length === 0 && (
              <tr>
                <td colSpan={7} className="px-5 py-8 text-center text-gray-500">Kullanıcı bulunamadı.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
