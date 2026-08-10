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
  const [addingBalance, setAddingBalance] = useState(false);
  const [balanceAmount, setBalanceAmount] = useState('');
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

  const handleAddBalance = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    const amount = Number(balanceAmount);
    if (!Number.isFinite(amount) || amount <= 0) {
      setError('Geçerli bir tutar girin (0’dan büyük olmalı).');
      return;
    }
    try {
      await usersApi.addBalance(amount);
      await refreshUser();
      setAddingBalance(false);
      setBalanceAmount('');
      setMessage(`${amount.toFixed(2)} ₺ bakiye yüklendi.`);
    } catch (err: any) {
      setError(err.response?.data?.message || err.response?.data || 'Bakiye yükleme başarısız');
    }
  };

  if (!user) return null;

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="page-title">Profilim</h1>

      {message && <div className="success-banner mb-6">{message}</div>}
      {error && <div className="error-banner mb-6">{error}</div>}

      <div className="card mb-6">
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
              <div>
                <p className="text-xs text-gray-500">Bakiye</p>
                <p className="font-semibold text-green-700">{(user.balance ?? 0).toFixed(2)} ₺</p>
              </div>
            </div>
            <div className="flex gap-2 pt-4">
              <button onClick={() => setEditing(true)} className="btn-primary">
                Düzenle
              </button>
              <button onClick={() => setChangingPassword(true)} className="btn-secondary">
                Şifre Değiştir
              </button>
              <button onClick={() => setAddingBalance(true)} className="btn-primary bg-green-600 hover:bg-green-700">
                Bakiye Yükle
              </button>
            </div>
          </div>
        ) : (
          <form onSubmit={handleUpdate} className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div className="form-group">
                <label className="label">Ad</label>
                <input
                  value={form.firstName}
                  onChange={(e) => setForm((p) => ({ ...p, firstName: e.target.value }))}
                  className="input"
                />
              </div>
              <div className="form-group">
                <label className="label">Soyad</label>
                <input
                  value={form.lastName}
                  onChange={(e) => setForm((p) => ({ ...p, lastName: e.target.value }))}
                  className="input"
                />
              </div>
            </div>
            <div className="form-group">
              <label className="label">E-posta</label>
              <input
                type="email"
                value={form.email}
                onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
                className="input"
              />
            </div>
            <div className="form-group">
              <label className="label">Telefon</label>
              <input
                value={form.phoneNumber}
                onChange={(e) => setForm((p) => ({ ...p, phoneNumber: e.target.value }))}
                className="input"
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="btn-primary">Kaydet</button>
              <button type="button" onClick={() => setEditing(false)} className="btn-secondary">İptal</button>
            </div>
          </form>
        )}
      </div>

      {/* Add Balance */}
      {addingBalance && (
        <div className="card mb-6">
          <h2 className="card-title mb-1">Bakiye Yükle</h2>
          <p className="text-xs text-gray-500 mb-4">Mevcut bakiye: {(user.balance ?? 0).toFixed(2)} ₺</p>
          <form onSubmit={handleAddBalance} className="space-y-3">
            <div className="form-group">
              <label className="label">Tutar (₺)</label>
              <input
                type="number"
                min="0.01"
                step="0.01"
                value={balanceAmount}
                onChange={(e) => setBalanceAmount(e.target.value)}
                required
                placeholder="100.00"
                className="input"
              />
            </div>
            <div className="flex gap-2 flex-wrap">
              {[50, 100, 250, 500].map((amount) => (
                <button
                  key={amount}
                  type="button"
                  onClick={() => setBalanceAmount(String(amount))}
                  className="btn-secondary btn-sm"
                >
                  +{amount} ₺
                </button>
              ))}
            </div>
            <div className="flex gap-2">
              <button type="submit" className="btn-primary bg-green-600 hover:bg-green-700">Yükle</button>
              <button
                type="button"
                onClick={() => { setAddingBalance(false); setBalanceAmount(''); }}
                className="btn-secondary"
              >
                İptal
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Change Password */}
      {changingPassword && (
        <div className="card">
          <h2 className="card-title mb-4">Şifre Değiştir</h2>
          <form onSubmit={handleChangePassword} className="space-y-3">
            <div className="form-group">
              <label className="label">Mevcut Şifre</label>
              <input
                type="password"
                value={passwordForm.currentPassword}
                onChange={(e) => setPasswordForm((p) => ({ ...p, currentPassword: e.target.value }))}
                required
                className="input"
              />
            </div>
            <div className="form-group">
              <label className="label">Yeni Şifre</label>
              <input
                type="password"
                value={passwordForm.newPassword}
                onChange={(e) => setPasswordForm((p) => ({ ...p, newPassword: e.target.value }))}
                required
                className="input"
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="btn-primary">Şifreyi Değiştir</button>
              <button type="button" onClick={() => setChangingPassword(false)} className="btn-secondary">İptal</button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
