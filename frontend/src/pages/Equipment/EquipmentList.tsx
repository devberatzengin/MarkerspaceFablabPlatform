import { useEffect, useState } from 'react';
import { equipmentApi } from '../../api/equipment';
import { useAuth } from '../../context/AuthContext';
import DetailModal, { DetailRow } from '../../components/DetailModal';
import type {
  EquipmentResponse,
  EquipmentStatus,
  EquipmentType,
  EquipmentPlacementType,
  PagedResponse,
} from '../../types';

const statusLabel: Record<EquipmentStatus, string> = {
  Unknown: 'Bilinmiyor',
  Available: 'Kullanılabilir',
  Reserved: 'Rezerve',
  Rented: 'Kirada',
  Maintenance: 'Bakımda',
};

const statusColor: Record<EquipmentStatus, string> = {
  Unknown: 'bg-gray-100 text-gray-800',
  Available: 'bg-green-100 text-green-800',
  Reserved: 'bg-yellow-100 text-yellow-800',
  Rented: 'bg-blue-100 text-blue-800',
  Maintenance: 'bg-red-100 text-red-800',
};

const typeLabel: Record<EquipmentType, string> = {
  Unknown: 'Bilinmiyor',
  DigitalFabrication: 'Dijital Üretim',
  HandTools: 'El Aletleri',
  PowerTools: 'Elektrikli Aletler',
  Electronics: 'Elektronik',
  Measurement: 'Ölçüm',
};

const placementLabel: Record<EquipmentPlacementType, string> = {
  Unknown: 'Bilinmiyor',
  Portable: 'Taşınabilir',
  Benchtop: 'Tezgah Üstü',
  FloorStationary: 'Sabit',
};

const typeOptions: EquipmentType[] = ['DigitalFabrication', 'HandTools', 'PowerTools', 'Electronics', 'Measurement'];
const statusOptions: EquipmentStatus[] = ['Available', 'Reserved', 'Rented', 'Maintenance'];
const placementOptions: EquipmentPlacementType[] = ['Portable', 'Benchtop', 'FloorStationary'];

export default function EquipmentList() {
  const { isAdmin, user } = useAuth();
  const [data, setData] = useState<PagedResponse<EquipmentResponse> | null>(null);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [rentModal, setRentModal] = useState<{ id: string; action: 'rent' | 'reserve' } | null>(null);
  const [duration, setDuration] = useState('1:00:00');
  const [form, setForm] = useState({
    name: '',
    description: '',
    type: 'HandTools' as EquipmentType,
    placementType: 'Portable' as EquipmentPlacementType,
    requiredUserLevel: 1,
  });
  const [detailItem, setDetailItem] = useState<EquipmentResponse | null>(null);
  const [error, setError] = useState('');

  const openDetail = async (id: string) => {
    try {
      const item = await equipmentApi.getById(id);
      setDetailItem(item);
    } catch {}
  };

  const fetchData = async () => {
    try {
      const params: any = { page, pageSize: 10 };
      if (search) params.search = search;
      if (statusFilter) params.status = statusFilter;
      if (typeFilter) params.type = typeFilter;
      const res = await equipmentApi.getAll(params);
      setData(res);
    } catch {
      setData(null);
    }
  };

  useEffect(() => {
    fetchData();
  }, [page, search, statusFilter, typeFilter]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await equipmentApi.create(form);
      setShowCreate(false);
      setForm({ name: '', description: '', type: 'HandTools', placementType: 'Portable', requiredUserLevel: 1 });
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Oluşturma başarısız');
    }
  };

  const handleRentReserve = async () => {
    if (!rentModal) return;
    try {
      if (rentModal.action === 'rent') {
        await equipmentApi.rent(rentModal.id, duration);
      } else {
        await equipmentApi.reserve(rentModal.id, duration);
      }
      setRentModal(null);
      setError('');
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || err.response?.data || 'İşlem başarısız');
    }
  };

  const handleRelease = async (id: string) => {
    try {
      await equipmentApi.release(id);
      fetchData();
    } catch {}
  };

  const handleMaintenance = async (id: string) => {
    try {
      await equipmentApi.maintenance(id);
      fetchData();
    } catch {}
  };

  const handleUnmaintenance = async (id: string) => {
    try {
      await equipmentApi.unmaintenance(id);
      fetchData();
    } catch {}
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Bu ekipmanı silmek istediğinize emin misiniz?')) return;
    try {
      await equipmentApi.delete(id);
      fetchData();
    } catch {}
  };

  return (
    <div>
      {/* Detail Modal */}
      {detailItem && (
        <DetailModal title="Ekipman Detayı" onClose={() => setDetailItem(null)}>
          <DetailRow label="ID" value={detailItem.id} />
          <DetailRow label="Ad" value={detailItem.name} />
          <DetailRow label="Açıklama" value={detailItem.description} />
          <DetailRow label="Tip" value={typeLabel[detailItem.type]} />
          <DetailRow label="Yerleşim" value={placementLabel[detailItem.placementType]} />
          <DetailRow label="Durum" value={statusLabel[detailItem.status]} badge={statusColor[detailItem.status]} />
          <DetailRow label="Gerekli Seviye" value={detailItem.requiredUserLevel} />
          <DetailRow label="Mevcut Kullanıcı ID" value={detailItem.currentUserId} />
          <DetailRow label="Kullanılabilir Tarih" value={new Date(detailItem.availableAt).toLocaleString('tr-TR')} />
          <DetailRow label="Silinmiş" value={detailItem.isDeleted ? 'Evet' : 'Hayır'} />
        </DetailModal>
      )}

      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Ekipmanlar</h1>
        {isAdmin && (
          <button
            onClick={() => setShowCreate(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm"
          >
            Yeni Ekipman
          </button>
        )}
      </div>

      {error && <div className="bg-red-50 text-red-600 p-3 rounded mb-4 text-sm">{error}</div>}

      {/* Filters */}
      <div className="flex gap-3 mb-4 flex-wrap">
        <input
          type="text"
          placeholder="Ara..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="border border-gray-300 rounded-md px-3 py-2 text-sm flex-1 min-w-[150px] max-w-xs focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Tüm Durumlar</option>
          {statusOptions.map((s) => (
            <option key={s} value={s}>{statusLabel[s]}</option>
          ))}
        </select>
        <select
          value={typeFilter}
          onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }}
          className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Tüm Tipler</option>
          {typeOptions.map((t) => (
            <option key={t} value={t}>{typeLabel[t]}</option>
          ))}
        </select>
      </div>

      {/* Rent/Reserve Modal */}
      {rentModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm">
            <h3 className="font-semibold text-gray-900 mb-4">
              {rentModal.action === 'rent' ? 'Kirala' : 'Rezerve Et'}
            </h3>
            <label className="block text-sm text-gray-700 mb-2">Süre (saat:dakika:saniye)</label>
            <input
              value={duration}
              onChange={(e) => setDuration(e.target.value)}
              placeholder="1:00:00"
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <div className="flex gap-2">
              <button onClick={handleRentReserve} className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm">
                Onayla
              </button>
              <button onClick={() => setRentModal(null)} className="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-md text-sm">
                İptal
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Create Form */}
      {showCreate && isAdmin && (
        <div className="bg-white rounded-lg shadow p-5 mb-6">
          <h2 className="font-semibold text-gray-900 mb-4">Yeni Ekipman</h2>
          <form onSubmit={handleCreate} className="space-y-3">
            <input
              placeholder="İsim"
              value={form.name}
              onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              placeholder="Açıklama"
              value={form.description}
              onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <div className="grid grid-cols-3 gap-3">
              <select
                value={form.type}
                onChange={(e) => setForm((p) => ({ ...p, type: e.target.value as EquipmentType }))}
                className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {typeOptions.map((t) => (
                  <option key={t} value={t}>{typeLabel[t]}</option>
                ))}
              </select>
              <select
                value={form.placementType}
                onChange={(e) => setForm((p) => ({ ...p, placementType: e.target.value as EquipmentPlacementType }))}
                className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {placementOptions.map((p) => (
                  <option key={p} value={p}>{placementLabel[p]}</option>
                ))}
              </select>
              <input
                type="number"
                min={1}
                max={10}
                value={form.requiredUserLevel}
                onChange={(e) => setForm((p) => ({ ...p, requiredUserLevel: Number(e.target.value) }))}
                className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Gerekli Seviye"
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm">Oluştur</button>
              <button type="button" onClick={() => setShowCreate(false)} className="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-md text-sm">İptal</button>
            </div>
          </form>
        </div>
      )}

      {/* Equipment Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {data?.items.map((eq) => (
          <div key={eq.id} className="bg-white rounded-lg shadow p-5 cursor-pointer hover:shadow-md transition-shadow" onClick={() => openDetail(eq.id)}>
            <div className="flex justify-between items-start mb-3">
              <h3 className="font-semibold text-gray-900">{eq.name}</h3>
              <span className={`text-xs px-2 py-1 rounded-full font-medium ${statusColor[eq.status]}`}>
                {statusLabel[eq.status]}
              </span>
            </div>
            <p className="text-sm text-gray-500 mb-2">{eq.description}</p>
            <div className="text-xs text-gray-400 space-y-1 mb-3">
              <p>Tip: {typeLabel[eq.type]}</p>
              <p>Yerleşim: {placementLabel[eq.placementType]}</p>
              <p>Gerekli Seviye: {eq.requiredUserLevel}</p>
            </div>
            <div className="flex gap-2 flex-wrap" onClick={(e) => e.stopPropagation()}>
              {eq.status === 'Available' && eq.placementType === 'Portable' && (
                <button
                  onClick={() => setRentModal({ id: eq.id, action: 'rent' })}
                  className="bg-blue-600 hover:bg-blue-700 text-white px-3 py-1 rounded text-xs"
                >
                  Kirala
                </button>
              )}
              {eq.status === 'Available' &&
                (eq.placementType === 'Benchtop' || eq.placementType === 'FloorStationary') && (
                  <button
                    onClick={() => setRentModal({ id: eq.id, action: 'reserve' })}
                    className="bg-yellow-500 hover:bg-yellow-600 text-white px-3 py-1 rounded text-xs"
                  >
                    Rezerve Et
                  </button>
                )}
              {(eq.status === 'Rented' || eq.status === 'Reserved') &&
                eq.currentUserId === user?.id && (
                  <button
                    onClick={() => handleRelease(eq.id)}
                    className="bg-green-600 hover:bg-green-700 text-white px-3 py-1 rounded text-xs"
                  >
                    İade Et
                  </button>
                )}
              {isAdmin && (
                <>
                  {eq.status !== 'Maintenance' && (
                    <button onClick={() => handleMaintenance(eq.id)} className="bg-orange-500 hover:bg-orange-600 text-white px-3 py-1 rounded text-xs">
                      Bakıma Al
                    </button>
                  )}
                  {eq.status === 'Maintenance' && (
                    <button onClick={() => handleUnmaintenance(eq.id)} className="bg-green-600 hover:bg-green-700 text-white px-3 py-1 rounded text-xs">
                      Bakımdan Çıkar
                    </button>
                  )}
                  <button onClick={() => handleDelete(eq.id)} className="bg-red-600 hover:bg-red-700 text-white px-3 py-1 rounded text-xs">
                    Sil
                  </button>
                </>
              )}
            </div>
          </div>
        ))}
        {data?.items.length === 0 && (
          <div className="col-span-full text-center text-gray-500 py-8">Ekipman bulunamadı.</div>
        )}
      </div>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-6">
          <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-50">
            Önceki
          </button>
          <span className="px-3 py-1 text-sm text-gray-500">{page} / {data.totalPages}</span>
          <button onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))} disabled={page === data.totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-50">
            Sonraki
          </button>
        </div>
      )}
    </div>
  );
}
