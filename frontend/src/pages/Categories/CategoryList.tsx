import { useEffect, useState } from 'react';
import { categoriesApi } from '../../api/categories';
import DetailModal, { DetailRow } from '../../components/DetailModal';
import type { CategoryResponse, CategoryType } from '../../types';

const typeLabel: Record<CategoryType, string> = {
  Undefined: 'Tanımsız',
  Draft: 'Taslak',
  Published: 'Yayında',
  Unpublished: 'Yayından Kaldırıldı',
  Archived: 'Arşivlendi',
};

const typeOptions: CategoryType[] = ['Undefined', 'Draft', 'Published', 'Unpublished', 'Archived'];

export default function CategoryList() {
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [showInactive, setShowInactive] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [editItem, setEditItem] = useState<CategoryResponse | null>(null);
  const [form, setForm] = useState<{ name: string; type: CategoryType }>({ name: '', type: 'Published' });
  const [detailItem, setDetailItem] = useState<CategoryResponse | null>(null);
  const [error, setError] = useState('');

  const openDetail = async (id: string) => {
    try {
      const item = await categoriesApi.getById(id, true);
      setDetailItem(item);
    } catch {}
  };

  const fetchData = async () => {
    try {
      const data = await categoriesApi.getAll(showInactive);
      setCategories(data);
    } catch {}
  };

  useEffect(() => {
    fetchData();
  }, [showInactive]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await categoriesApi.create(form);
      setShowForm(false);
      setForm({ name: '', type: 'Published' });
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Oluşturma başarısız');
    }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editItem) return;
    setError('');
    try {
      await categoriesApi.update({ id: editItem.id, name: form.name, type: form.type, isActive: editItem.isActive });
      setEditItem(null);
      setForm({ name: '', type: 'Published' });
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Güncelleme başarısız');
    }
  };

  const handleDeactivate = async (id: string) => {
    try {
      await categoriesApi.deactivate(id);
      fetchData();
    } catch {}
  };


  const handleActivate = async (id: string) => {
    try {
      await categoriesApi.activate(id);
      fetchData();
    } catch {}
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Bu kategoriyi silmek istediğinize emin misiniz?')) return;
    try {
      await categoriesApi.delete(id);
      fetchData();
    } catch {}
  };

  const openEdit = (item: CategoryResponse) => {
    setEditItem(item);
    setForm({ name: item.name, type: item.type });
    setShowForm(false);
  };

  return (
    <div>
      {detailItem && (
        <DetailModal title="Kategori Detayı" onClose={() => setDetailItem(null)}>
          <DetailRow label="ID" value={detailItem.id} />
          <DetailRow label="Ad" value={detailItem.name} />
          <DetailRow label="Tip" value={typeLabel[detailItem.type]} />
          <DetailRow label="Durum" value={detailItem.isActive ? 'Aktif' : 'Pasif'} badge={detailItem.isActive ? 'badge-active' : 'badge-inactive'} />
        </DetailModal>
      )}

      <div className="flex justify-between items-center mb-6">
        <h1 className="page-title mb-0">Kategoriler</h1>
        <div className="flex gap-2">
          <button
            onClick={() => setShowInactive((v) => !v)}
            className="btn-secondary"
          >
            {showInactive ? 'Pasifleri Gizle' : 'Pasifleri Göster'}
          </button>
          <button
            onClick={() => { setShowForm(true); setEditItem(null); setForm({ name: '', type: 'Published' }); }}
            className="btn-primary"
          >
            Yeni Kategori
          </button>
        </div>
      </div>

      {error && <div className="error-banner mb-4">{error}</div>}

      {(showForm || editItem) && (
        <div className="card mb-6">
          <h2 className="card-title">{editItem ? 'Kategori Düzenle' : 'Yeni Kategori'}</h2>
          <form onSubmit={editItem ? handleUpdate : handleCreate} className="space-y-3">
            <input
              placeholder="Kategori Adı"
              value={form.name}
              onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
              required
              className="input"
            />
            <select
              value={form.type}
              onChange={(e) => setForm((p) => ({ ...p, type: e.target.value as CategoryType }))}
              className="select"
            >
              {typeOptions.map((t) => (
                <option key={t} value={t}>{typeLabel[t]}</option>
              ))}
            </select>
            <div className="flex gap-2">
              <button type="submit" className="btn-primary">
                {editItem ? 'Güncelle' : 'Oluştur'}
              </button>
              <button type="button" onClick={() => { setShowForm(false); setEditItem(null); setError(''); }} className="btn-secondary">
                İptal
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="card p-0 overflow-hidden">
        <table className="table">
          <thead>
            <tr>
              <th>Ad</th>
              <th>Tip</th>
              <th>Durum</th>
              <th>İşlem</th>
            </tr>
          </thead>
          <tbody>
            {categories.map((c) => (
              <tr key={c.id} className="cursor-pointer" onClick={() => openDetail(c.id)}>
                <td className="font-medium text-gray-900">{c.name}</td>
                <td className="text-gray-500">{typeLabel[c.type] ?? 'Bilinmiyor'}</td>
                <td>
                  <span className={`badge ${c.isActive ? 'badge-active' : 'badge-inactive'}`}>
                    {c.isActive ? 'Aktif' : 'Pasif'}
                  </span>
                </td>
                <td onClick={(e) => e.stopPropagation()}>
                  <div className="flex gap-2">
                    <button onClick={() => openEdit(c)} className="text-blue-600 hover:underline text-xs">Düzenle</button>
                    {c.isActive && (
                      <button onClick={() => handleDeactivate(c.id)} className="text-yellow-600 hover:underline text-xs">Pasifleştir</button>
                    )}
                    {!c.isActive && (
                        <button onClick={() => handleActivate(c.id)} className="text-green-600 hover:underline text-xs">Aktifleştir</button>
                    )}
                    <button onClick={() => handleDelete(c.id)} className="btn-danger">Sil</button>
                  </div>
                </td>
              </tr>
            ))}
            {categories.length === 0 && (
              <tr>
                <td colSpan={4} className="px-5 py-8 text-center text-gray-500">Kategori bulunamadı.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
