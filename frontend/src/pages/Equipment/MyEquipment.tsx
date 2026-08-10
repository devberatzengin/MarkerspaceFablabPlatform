import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { equipmentApi } from '../../api/equipment';
import DetailModal from '../../components/DetailModal';
import type { EquipmentRentalResponse, PagedResponse } from '../../types';

export default function MyEquipment() {
  const navigate = useNavigate();
  const [data, setData] = useState<PagedResponse<EquipmentRentalResponse> | null>(null);
  const [page, setPage] = useState(1);
  const [includePast, setIncludePast] = useState(false);
  // İade sonrası "şimdi mi ödeyeceksin" sorusu için teslim edilen kiralamanın id'si.
  const [justReleasedRentalId, setJustReleasedRentalId] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      const res = await equipmentApi.myEquipments({ page, pageSize: 10, includePast });
      setData(res);
    } catch {
      setData(null);
    }
  };

  useEffect(() => {
    fetchData();
  }, [page, includePast]);

  const handleRelease = async (rental: EquipmentRentalResponse) => {
    try {
      await equipmentApi.release(rental.equipmentId);
      await fetchData();
      setJustReleasedRentalId(rental.id);
    } catch {}
  };

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="page-title mb-0">Ekipmanlarım</h1>
        <label className="flex items-center gap-2 text-sm text-gray-600">
          <input
            type="checkbox"
            checked={includePast}
            onChange={(e) => { setIncludePast(e.target.checked); setPage(1); }}
            className="rounded"
          />
          Geçmiş kiralamaları göster
        </label>
      </div>

      <div className="card p-0 overflow-hidden">
        <table className="table">
          <thead>
            <tr>
              <th>Ekipman</th>
              <th className="hidden md:table-cell">Açıklama</th>
              <th>Kiralama Tarihi</th>
              <th className="hidden md:table-cell">İade Edilmesi Gereken Tarih</th>
              <th className="hidden md:table-cell">İade Edilen Tarih</th>
              <th>Durum</th>
              <th>İşlem</th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((r) => (
              <tr key={r.id}>
                <td className="font-medium text-gray-900">{r.equipmentName}</td>
                <td className="hidden md:table-cell text-gray-500">{r.equipmentDescription}</td>
                <td className="text-gray-500">
                  {new Date(r.rentedAt).toLocaleString('tr-TR')}
                </td>
                <td className="hidden md:table-cell text-gray-500">
                  {new Date(r.expectedReturnAt).toLocaleString('tr-TR')}
                </td>
                <td className="hidden md:table-cell text-gray-500">
                  {r.releasedAt ? new Date(r.releasedAt).toLocaleString('tr-TR') : '—'}
                </td>
                <td>
                  <span className={`badge ${r.isActive ? 'badge-active' : 'badge-inactive'}`}>
                    {r.isActive ? 'Aktif' : 'İade Edildi'}
                  </span>
                </td>
                <td>
                  {r.isActive && (
                    <button
                      onClick={() => handleRelease(r)}
                      className="text-blue-600 hover:underline text-xs"
                    >
                      İade Et
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {data?.items.length === 0 && (
              <tr>
                <td colSpan={7} className="px-6 py-8 text-center text-gray-500">Kiralama kaydı bulunamadı.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-4">
          <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="btn-secondary disabled:opacity-50">Önceki</button>
          <span className="px-3 py-1 text-sm text-gray-500">{page} / {data.totalPages}</span>
          <button onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))} disabled={page === data.totalPages} className="btn-secondary disabled:opacity-50">Sonraki</button>
        </div>
      )}

      {justReleasedRentalId && (
        <DetailModal title="İade Tamamlandı" onClose={() => setJustReleasedRentalId(null)}>
          <p className="text-sm text-gray-700">
            Ekipman iade edildi. Ödemenizi şimdi yapmak ister misiniz?
          </p>
          <p className="text-sm text-gray-500 mt-2">
            Sonra derseniz bu ödeme <span className="font-medium">Ödemelerim</span> sayfasında bekleyen
            ödemeler arasında görünmeye devam eder.
          </p>

          <div className="flex justify-end gap-2 mt-5">
            <button
              onClick={() => setJustReleasedRentalId(null)}
              className="btn-secondary"
            >
              Sonra
            </button>
            <button
              onClick={() => navigate(`/payments?pay=${justReleasedRentalId}`)}
              className="btn-primary"
            >
              Şimdi Öde
            </button>
          </div>
        </DetailModal>
      )}
    </div>
  );
}
