import { useEffect, useState } from 'react';
import { announcementsApi } from '../../api/announcements';
import { categoriesApi } from '../../api/categories';
import { useAuth } from '../../context/AuthContext';
import DetailModal, { DetailRow } from '../../components/DetailModal';
import type { AnnouncementResponse, CategoryResponse, ContentStatus, PagedResponse } from '../../types';

const statusLabel: Record<ContentStatus, string> = {
  Draft: 'Taslak',
  Published: 'Yayında',
  Unpublished: 'Yayından Kaldırıldı',
  Archived: 'Arşivlendi',
};

const statusBadge: Record<ContentStatus, string> = {
  Draft: 'badge-draft',
  Published: 'badge-published',
  Unpublished: 'badge-pending',
  Archived: 'badge-archived',
};

export default function AnnouncementList() {
  const { isAdmin } = useAuth();
  const [data, setData] = useState<PagedResponse<AnnouncementResponse> | null>(null);
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [editItem, setEditItem] = useState<AnnouncementResponse | null>(null);
  const [detailItem, setDetailItem] = useState<AnnouncementResponse | null>(null);
  const [form, setForm] = useState({ title: '', content: '', categoryId: '' });
  const [error, setError] = useState('');

  const fetchData = async () => {
    try {
      const params: any = { page, pageSize: 10 };
      if (search) params.search = search;
      if (categoryFilter) params.categoryId = categoryFilter;
      if (statusFilter) params.status = statusFilter;
      const res = await announcementsApi.getAll(params);
      setData(res);
    } catch {
      setData(null);
    }
  };

  useEffect(() => {
    categoriesApi.getAll().then(setCategories).catch(() => {});
  }, []);

  useEffect(() => {
    fetchData();
  }, [page, search, categoryFilter, statusFilter]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await announcementsApi.create(form);
      setShowCreate(false);
      setForm({ title: '', content: '', categoryId: '' });
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
      await announcementsApi.update(editItem.id, { id: editItem.id, ...form });
      setEditItem(null);
      setForm({ title: '', content: '', categoryId: '' });
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Güncelleme başarısız');
    }
  };

  const handleAction = async (id: string, action: 'publish' | 'unpublish' | 'archive') => {
    try {
      await announcementsApi[action](id);
      fetchData();
    } catch {}
  };

  const openEdit = (item: AnnouncementResponse) => {
    setEditItem(item);
    setForm({ title: item.title, content: item.content, categoryId: item.categoryId });
    setShowCreate(false);
  };

  const openDetail = async (id: string) => {
    try {
      const item = await announcementsApi.getById(id);
      setDetailItem(item);
    } catch {}
  };

  return (
    <div>
      {/* Detail Modal */}
      {detailItem && (
        <DetailModal title="Duyuru Detayı" onClose={() => setDetailItem(null)}>
          <DetailRow label="ID" value={detailItem.id} />
          <DetailRow label="Başlık" value={detailItem.title} />
          <DetailRow label="İçerik" value={detailItem.content} />
          <DetailRow label="Kategori" value={detailItem.categoryName} />
          <DetailRow label="Kategori ID" value={detailItem.categoryId} />
          <DetailRow label="Yazan" value={detailItem.createdByName} />
          <DetailRow label="Yazan ID" value={detailItem.createdByUserId} />
          <DetailRow label="Durum" value={statusLabel[detailItem.status]} badge={statusBadge[detailItem.status]} />
          <DetailRow label="Oluşturulma" value={new Date(detailItem.createdAt).toLocaleString('tr-TR')} />
          <DetailRow label="Güncellenme" value={new Date(detailItem.updatedAt).toLocaleString('tr-TR')} />
        </DetailModal>
      )}

      <div className="flex justify-between items-center mb-6">
        <h1 className="page-title mb-0">Duyurular</h1>
        <button
          onClick={() => { setShowCreate(true); setEditItem(null); setForm({ title: '', content: '', categoryId: '' }); }}
          className="btn-primary"
        >
          Yeni Duyuru
        </button>
      </div>

      <div className="flex gap-3 mb-4 flex-wrap">
        <input
          type="text"
          placeholder="Ara..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="input flex-1 min-w-[150px] max-w-xs"
        />
        <select
          value={categoryFilter}
          onChange={(e) => { setCategoryFilter(e.target.value); setPage(1); }}
          className="select"
        >
          <option value="">Tüm Kategoriler</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          className="select"
        >
          <option value="">Tüm Durumlar</option>
          {(['Draft', 'Published', 'Unpublished', 'Archived'] as ContentStatus[]).map((s) => (
            <option key={s} value={s}>{statusLabel[s]}</option>
          ))}
        </select>
      </div>

      {(showCreate || editItem) && (
        <div className="card mb-6">
          <h2 className="card-title">{editItem ? 'Duyuru Düzenle' : 'Yeni Duyuru'}</h2>
          {error && <div className="error-banner mb-3">{error}</div>}
          <form onSubmit={editItem ? handleUpdate : handleCreate} className="space-y-3">
            <input
              placeholder="Başlık"
              value={form.title}
              onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
              required
              className="input"
            />
            <textarea
              placeholder="İçerik"
              value={form.content}
              onChange={(e) => setForm((p) => ({ ...p, content: e.target.value }))}
              rows={3}
              className="textarea"
            />
            <select
              value={form.categoryId}
              onChange={(e) => setForm((p) => ({ ...p, categoryId: e.target.value }))}
              required
              className="select"
            >
              <option value="">Kategori Seç</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
            <div className="flex gap-2">
              <button type="submit" className="btn-primary">
                {editItem ? 'Güncelle' : 'Oluştur'}
              </button>
              <button
                type="button"
                onClick={() => { setShowCreate(false); setEditItem(null); setError(''); }}
                className="btn-secondary"
              >
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
              <th>Başlık</th>
              <th className="hidden md:table-cell">Kategori</th>
              <th className="hidden md:table-cell">Yazan</th>
              <th>Durum</th>
              <th className="hidden md:table-cell">Tarih</th>
              <th>İşlem</th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((a) => (
              <tr key={a.id} className="cursor-pointer" onClick={() => openDetail(a.id)}>
                <td className="font-medium text-gray-900">{a.title}</td>
                <td className="hidden md:table-cell text-gray-500">{a.categoryName}</td>
                <td className="hidden md:table-cell text-gray-500">{a.createdByName}</td>
                <td>
                  <span className={`badge ${statusBadge[a.status] ?? 'badge-inactive'}`}>
                    {statusLabel[a.status] ?? ''}
                  </span>
                </td>
                <td className="hidden md:table-cell text-gray-500">
                  {new Date(a.createdAt).toLocaleString('tr-TR')}
                </td>
                <td onClick={(e) => e.stopPropagation()}>
                  <div className="flex gap-2 flex-wrap">
                    <button onClick={() => openEdit(a)} className="text-blue-600 hover:underline text-xs">Düzenle</button>
                    {isAdmin && (a.status === 'Draft' || a.status === 'Unpublished' )  && (
                      <button onClick={() => handleAction(a.id, 'publish')} className="text-green-600 hover:underline text-xs">Yayınla</button>
                    )}
                    {isAdmin && a.status === 'Published' && (
                      <button onClick={() => handleAction(a.id, 'unpublish')} className="text-yellow-600 hover:underline text-xs">Kaldır</button>
                    )}
                    {isAdmin && a.status !== 'Archived' && (
                      <button onClick={() => handleAction(a.id, 'archive')} className="btn-danger">Arşivle</button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {data?.items.length === 0 && (
              <tr>
                <td colSpan={6} className="px-5 py-8 text-center text-gray-500">Duyuru bulunamadı.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-4">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="btn-secondary disabled:opacity-50"
          >
            Önceki
          </button>
          <span className="px-3 py-1 text-sm text-gray-500">
            {page} / {data.totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
            disabled={page === data.totalPages}
            className="btn-secondary disabled:opacity-50"
          >
            Sonraki
          </button>
        </div>
      )}
    </div>
  );
}
