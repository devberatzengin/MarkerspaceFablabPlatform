import { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { usersApi } from '../../api/users';
import type { MembershipStatus } from '../../types';

const membershipLabel: Record<MembershipStatus, string> = {
  Unknown: 'Bilinmiyor',
  Free: 'Ücretsiz',
  Bronze: 'Bronz',
  Silver: 'Gümüş',
  Gold: 'Altın',
  Professional: 'Profesyonel',
};

export default function Profile() {
  const { user, isAdmin, refreshUser } = useAuth();
  const [editing, setEditing] = useState(false);
  const [changingPassword, setChangingPassword] = useState(false);
  const [form, setForm] = useState({
    firstName: user?.firstName ?? '',
    lastName: user?.lastName ?? '',
    email: user?.email ?? '',
    phoneNumber: user?.phoneNumber ?? '',
  });
  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', newPassword: '' });
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await usersApi.update(user!.id, { ...form, status: user!.status });
      await refreshUser();
      setEditing(false);
      setMessage('Profil güncellendi.');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Güncelleme başarısız');
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await usersApi.changePassword(passwordForm);
      setChangingPassword(false);
      setPasswordForm({ currentPassword: '', newPassword: '' });
      setMessage('Şifre değiştirildi.');
    } catch (err: any) {
      setError(err.response?.data?.message || err.response?.data || 'Şifre değişikliği başarısız');
    }
  };

  if (!user) return null;

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Profilim</h1>

      {message && <div className="bg-green-50 text-green-600 p-3 rounded mb-4 text-sm">{message}</div>}
      {error && <div className="bg-red-50 text-red-600 p-3 rounded mb-4 text-sm">{error}</div>}

      <div className="bg-white rounded-lg shadow p-6 mb-6">
        {!editing ? (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-gray-500">Ad</p>
                <p className="font-medium text-gray-900">{user.firstName}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Soyad</p>
                <p className="font-medium text-gray-900">{user.lastName}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Kullanıcı Adı</p>
                <p className="font-medium text-gray-900">{user.username}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">E-posta</p>
                <p className="font-medium text-gray-900">{user.email}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Telefon</p>
                <p className="font-medium text-gray-900">{user.phoneNumber || '-'}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Rol</p>
                <p className="font-medium text-gray-900">{isAdmin ? 'Yönetici' : 'Kullanıcı'}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Üyelik</p>
                <p className="font-medium text-gray-900">{membershipLabel[user.status] ?? 'Bilinmiyor'}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Ekipman Seviyesi</p>
                <p className="font-medium text-gray-900">{user.equipmentLevel} / 10</p>
              </div>
            </div>
            <div className="flex gap-2 pt-4">
              <button onClick={() => setEditing(true)} className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm">
                Düzenle
              </button>
              <button onClick={() => setChangingPassword(true)} className="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-md text-sm">
                Şifre Değiştir
              </button>
            </div>
          </div>
        ) : (
          <form onSubmit={handleUpdate} className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-gray-500 mb-1">Ad</label>
                <input
                  value={form.firstName}
                  onChange={(e) => setForm((p) => ({ ...p, firstName: e.target.value }))}
                  className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-xs text-gray-500 mb-1">Soyad</label>
                <input
                  value={form.lastName}
                  onChange={(e) => setForm((p) => ({ ...p, lastName: e.target.value }))}
                  className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
            <div>
              <label className="block text-xs text-gray-500 mb-1">E-posta</label>
              <input
                type="email"
                value={form.email}
                onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
                className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-xs text-gray-500 mb-1">Telefon</label>
              <input
                value={form.phoneNumber}
                onChange={(e) => setForm((p) => ({ ...p, phoneNumber: e.target.value }))}
                className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm">Kaydet</button>
              <button type="button" onClick={() => setEditing(false)} className="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-md text-sm">İptal</button>
            </div>
          </form>
        )}
      </div>

      {/* Change Password */}
      {changingPassword && (
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="font-semibold text-gray-900 mb-4">Şifre Değiştir</h2>
          <form onSubmit={handleChangePassword} className="space-y-3">
            <div>
              <label className="block text-xs text-gray-500 mb-1">Mevcut Şifre</label>
              <input
                type="password"
                value={passwordForm.currentPassword}
                onChange={(e) => setPasswordForm((p) => ({ ...p, currentPassword: e.target.value }))}
                required
                className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-xs text-gray-500 mb-1">Yeni Şifre</label>
              <input
                type="password"
                value={passwordForm.newPassword}
                onChange={(e) => setPasswordForm((p) => ({ ...p, newPassword: e.target.value }))}
                required
                className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm">Şifreyi Değiştir</button>
              <button type="button" onClick={() => setChangingPassword(false)} className="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-md text-sm">İptal</button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
