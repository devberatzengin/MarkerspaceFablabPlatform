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
        <h1 className="text-2xl font-bold text-gray-900">Ekipmanlarım</h1>
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

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">Ekipman</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium hidden md:table-cell">Açıklama</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">Kiralama Tarihi</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium hidden md:table-cell">İade Edilmesi Gereken Tarih</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium hidden md:table-cell">İade Edilen Tarih</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">Durum</th>
              <th className="text-left px-5 py-3 text-gray-500 font-medium">İşlem</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {data?.items.map((r) => (
              <tr key={r.id} className="hover:bg-gray-50">
                <td className="px-5 py-3 font-medium text-gray-900">{r.equipmentName}</td>
                <td className="px-5 py-3 text-gray-500 hidden md:table-cell">{r.equipmentDescription}</td>
                <td className="px-5 py-3 text-gray-500">
                  {new Date(r.rentedAt).toLocaleString('tr-TR')}
                </td>
                <td className="px-5 py-3 text-gray-500 hidden md:table-cell">
                  {new Date(r.expectedReturnAt).toLocaleString('tr-TR')}
                </td>
                <td className="px-5 py-3 text-gray-500 hidden md:table-cell">
                  {r.releasedAt ? new Date(r.releasedAt).toLocaleString('tr-TR') : '—'}
                </td>
                <td className="px-5 py-3">
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${r.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
                    {r.isActive ? 'Aktif' : 'İade Edildi'}
                  </span>
                </td>
                <td className="px-5 py-3">
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
                <td colSpan={7} className="px-5 py-8 text-center text-gray-500">Kiralama kaydı bulunamadı.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-4">
          <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-50">Önceki</button>
          <span className="px-3 py-1 text-sm text-gray-500">{page} / {data.totalPages}</span>
          <button onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))} disabled={page === data.totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-50">Sonraki</button>
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
              className="px-4 py-2 rounded border text-sm text-gray-700 hover:bg-gray-50"
            >
              Sonra
            </button>
            <button
              onClick={() => navigate(`/payments?pay=${justReleasedRentalId}`)}
              className="px-4 py-2 rounded bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium"
            >
              Şimdi Öde
            </button>
          </div>
        </DetailModal>
      )}
    </div>
  );
}
