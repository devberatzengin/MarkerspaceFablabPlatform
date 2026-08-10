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

const statusBadge: Record<EquipmentStatus, string> = {
  Unknown: 'badge badge-inactive',
  Available: 'badge badge-available',
  Reserved: 'badge badge-reserved',
  Rented: 'badge badge-info',
  Maintenance: 'badge badge-maintenance',
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

/** Date -> datetime-local input'unun beklediği "YYYY-MM-DDTHH:mm" (yerel saat) formatı. */
const toLocalInputValue = (d: Date) => {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

/** İki tarih arası süreyi .NET TimeSpan formatına ("[d.]hh:mm:ss") çevirir. */
const toTimeSpan = (start: Date, end: Date) => {
  const pad = (n: number) => String(n).padStart(2, '0');
  const totalSeconds = Math.round((end.getTime() - start.getTime()) / 1000);
  const days = Math.floor(totalSeconds / 86400);
  const hours = Math.floor((totalSeconds % 86400) / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  const clock = `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
  return days > 0 ? `${days}.${clock}` : clock;
};

/** İki tarih arası süreyi "2 gün 3 saat" gibi okunur hale getirir. */
const formatDuration = (start: Date, end: Date) => {
  const totalMinutes = Math.round((end.getTime() - start.getTime()) / 60000);
  if (totalMinutes <= 0) return '-';
  const days = Math.floor(totalMinutes / 1440);
  const hours = Math.floor((totalMinutes % 1440) / 60);
  const minutes = totalMinutes % 60;
  return [days && `${days} gün`, hours && `${hours} saat`, minutes && `${minutes} dk`]
    .filter(Boolean)
    .join(' ');
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
  const [aheadModal, setAheadModal] = useState<{
    id: string;
    name: string;
    placementType: EquipmentPlacementType;
  } | null>(null);
  const [ahead, setAhead] = useState({ startAt: '', endAt: '' });
  const [aheadError, setAheadError] = useState('');
  const [aheadBusy, setAheadBusy] = useState(false);
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

  // Modal açılırken makul bir varsayılan doldur: yarın, tam saat başı, 2 saatlik pencere.
  const openAheadModal = (eq: EquipmentResponse) => {
    const start = new Date();
    start.setDate(start.getDate() + 1);
    start.setMinutes(0, 0, 0);
    const end = new Date(start);
    end.setHours(end.getHours() + 2);

    setAhead({ startAt: toLocalInputValue(start), endAt: toLocalInputValue(end) });
    setAheadError('');
    setAheadModal({ id: eq.id, name: eq.name, placementType: eq.placementType });
  };

  const handleReserveAhead = async () => {
    if (!aheadModal) return;

    if (!ahead.startAt || !ahead.endAt) {
      setAheadError('Başlangıç ve bitiş tarihi zorunlu.');
      return;
    }

    const start = new Date(ahead.startAt);
    const end = new Date(ahead.endAt);

    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
      setAheadError('Geçersiz tarih.');
      return;
    }
    if (start.getTime() <= Date.now()) {
      setAheadError('Başlangıç tarihi gelecekte olmalı.');
      return;
    }
    if (end <= start) {
      setAheadError('Bitiş tarihi başlangıçtan sonra olmalı.');
      return;
    }

    setAheadBusy(true);
    try {
      // Kolonlar "timestamp with time zone" olduğu için yerel saati UTC'ye çevirip gönderiyoruz.
      // Backend bitiş tarihi değil süre bekliyor: end - start -> TimeSpan.
      const payload = { startAt: start.toISOString(), span: toTimeSpan(start, end) };

      // Taşınabilir ekipman kiralanır, tezgah üstü/sabit ekipman rezerve edilir.
      if (aheadModal.placementType === 'Portable') {
        await equipmentApi.rentLater(aheadModal.id, payload);
      } else {
        await equipmentApi.reserveLater(aheadModal.id, payload);
      }
      setAheadModal(null);
      setError('');
      fetchData();
    } catch (err: any) {
      const data = err.response?.data;
      setAheadError(
        data?.detail || data?.message || (typeof data === 'string' ? data : '') || 'Rezervasyon başarısız.',
      );
    } finally {
      setAheadBusy(false);
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
          <DetailRow label="Durum" value={statusLabel[detailItem.status]} badge={statusBadge[detailItem.status]} />
          <DetailRow label="Gerekli Seviye" value={detailItem.requiredUserLevel} />
          <DetailRow label="Mevcut Kullanıcı ID" value={detailItem.currentUserId} />
          <DetailRow label="Kullanılabilir Tarih" value={new Date(detailItem.availableAt).toLocaleString('tr-TR')} />
          <DetailRow label="Silinmiş" value={detailItem.isDeleted ? 'Evet' : 'Hayır'} />
        </DetailModal>
      )}

      <div className="flex justify-between items-center mb-6">
        <h1 className="page-title mb-0">Ekipmanlar</h1>
        {isAdmin && (
          <button
            onClick={() => setShowCreate(true)}
            className="btn-primary"
          >
            Yeni Ekipman
          </button>
        )}
      </div>

      {error && <div className="error-banner mb-4">{error}</div>}

      {/* Filters */}
      <div className="flex gap-3 mb-4 flex-wrap">
        <input
          type="text"
          placeholder="Ara..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="input flex-1 min-w-[150px] max-w-xs"
        />
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          className="select"
        >
          <option value="">Tüm Durumlar</option>
          {statusOptions.map((s) => (
            <option key={s} value={s}>{statusLabel[s]}</option>
          ))}
        </select>
        <select
          value={typeFilter}
          onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }}
          className="select"
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
          <div className="card w-full max-w-sm">
            <h3 className="card-title mb-4">
              {rentModal.action === 'rent' ? 'Kirala' : 'Rezerve Et'}
            </h3>
            <label className="label">Süre (saat:dakika:saniye)</label>
            <input
              value={duration}
              onChange={(e) => setDuration(e.target.value)}
              placeholder="1:00:00"
              className="input mb-4"
            />
            <div className="flex gap-2">
              <button onClick={handleRentReserve} className="btn-primary">
                Onayla
              </button>
              <button onClick={() => setRentModal(null)} className="btn-secondary">
                İptal
              </button>
            </div>
          </div>
        </div>
      )}

      {/* İleri Tarihli Rezervasyon Modal */}
      {aheadModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="card w-full max-w-md">
            <h3 className="card-title mb-1">İleri Tarihe Al</h3>
            <p className="text-sm text-gray-500 mb-4">{aheadModal.name}</p>

            <label className="label">Başlangıç</label>
            <input
              type="datetime-local"
              value={ahead.startAt}
              min={toLocalInputValue(new Date())}
              onChange={(e) => setAhead((p) => ({ ...p, startAt: e.target.value }))}
              className="input mb-3"
            />

            <label className="label">Bitiş</label>
            <input
              type="datetime-local"
              value={ahead.endAt}
              min={ahead.startAt || toLocalInputValue(new Date())}
              onChange={(e) => setAhead((p) => ({ ...p, endAt: e.target.value }))}
              className="input mb-3"
            />

            {ahead.startAt && ahead.endAt && new Date(ahead.endAt) > new Date(ahead.startAt) && (
              <p className="text-xs text-gray-500 mb-3">
                Toplam süre: {formatDuration(new Date(ahead.startAt), new Date(ahead.endAt))}
              </p>
            )}

            {aheadError && (
              <div className="error-banner mb-3">{aheadError}</div>
            )}

            <div className="flex gap-2">
              <button
                onClick={handleReserveAhead}
                disabled={aheadBusy}
                className="btn-primary disabled:opacity-50"
              >
                {aheadBusy
                  ? 'Gönderiliyor...'
                  : aheadModal.placementType === 'Portable'
                    ? 'Kirala'
                    : 'Rezerve Et'}
              </button>
              <button
                onClick={() => setAheadModal(null)}
                className="btn-secondary"
              >
                İptal
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Create Form */}
      {showCreate && isAdmin && (
        <div className="card mb-6">
          <h2 className="card-title mb-4">Yeni Ekipman</h2>
          <form onSubmit={handleCreate} className="space-y-3">
            <input
              placeholder="İsim"
              value={form.name}
              onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
              required
              className="input"
            />
            <input
              placeholder="Açıklama"
              value={form.description}
              onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
              className="input"
            />
            <div className="grid grid-cols-3 gap-3">
              <select
                value={form.type}
                onChange={(e) => setForm((p) => ({ ...p, type: e.target.value as EquipmentType }))}
                className="select"
              >
                {typeOptions.map((t) => (
                  <option key={t} value={t}>{typeLabel[t]}</option>
                ))}
              </select>
              <select
                value={form.placementType}
                onChange={(e) => setForm((p) => ({ ...p, placementType: e.target.value as EquipmentPlacementType }))}
                className="select"
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
                className="input"
                placeholder="Gerekli Seviye"
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="btn-primary">Oluştur</button>
              <button type="button" onClick={() => setShowCreate(false)} className="btn-secondary">İptal</button>
            </div>
          </form>
        </div>
      )}

      {/* Equipment Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {data?.items.map((eq) => (
          <div key={eq.id} className="card cursor-pointer" onClick={() => openDetail(eq.id)}>
            <div className="flex justify-between items-start mb-3">
              <h3 className="font-semibold text-gray-900">{eq.name}</h3>
              <span className={statusBadge[eq.status]}>
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
                  className="btn-sm bg-blue-600 hover:bg-blue-700 text-white"
                >
                  Kirala
                </button>
              )}
              {eq.status === 'Available' &&
                (eq.placementType === 'Benchtop' || eq.placementType === 'FloorStationary') && (
                  <button
                    onClick={() => setRentModal({ id: eq.id, action: 'reserve' })}
                    className="btn-sm bg-yellow-500 hover:bg-yellow-600 text-white"
                  >
                    Rezerve Et
                  </button>
                )}
              {/* İleri tarihli rezervasyon anlık duruma bağlı değil: makine şu an kirada olsa
                  bile gelecekteki boş bir aralık için rezerve edilebilir. */}
              {eq.status !== 'Maintenance' && (
                <button
                  onClick={() => openAheadModal(eq)}
                  className="btn-sm bg-blue-500 hover:bg-blue-600 text-white"
                >
                  İleri Tarihe Al
                </button>
              )}
              {(eq.status === 'Rented' || eq.status === 'Reserved') &&
                eq.currentUserId === user?.id && (
                  <button
                    onClick={() => handleRelease(eq.id)}
                    className="btn-sm bg-green-600 hover:bg-green-700 text-white"
                  >
                    İade Et
                  </button>
                )}
              {isAdmin && (
                <>
                  {eq.status !== 'Maintenance' && (
                    <button onClick={() => handleMaintenance(eq.id)} className="btn-sm bg-orange-500 hover:bg-orange-600 text-white">
                      Bakıma Al
                    </button>
                  )}
                  {eq.status === 'Maintenance' && (
                    <button onClick={() => handleUnmaintenance(eq.id)} className="btn-sm bg-green-600 hover:bg-green-700 text-white">
                      Bakımdan Çıkar
                    </button>
                  )}
                  <button onClick={() => handleDelete(eq.id)} className="btn-danger">
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
          <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="btn-secondary disabled:opacity-50">
            Önceki
          </button>
          <span className="px-3 py-1 text-sm text-gray-500">{page} / {data.totalPages}</span>
          <button onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))} disabled={page === data.totalPages} className="btn-secondary disabled:opacity-50">
            Sonraki
          </button>
        </div>
      )}
    </div>
  );
}
