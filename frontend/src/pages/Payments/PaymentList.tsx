import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { paymentsApi } from '../../api/payments';
import DetailModal, { DetailRow } from '../../components/DetailModal';
import PayModal from '../../components/PayModal';
import { formatMoney, paymentStatusBadge, paymentStatusLabel } from '../../utils/payment';
import type { PagedResponse, PaymentResponse, PendingPaymentResponse } from '../../types';

export default function PaymentList() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [pending, setPending] = useState<PagedResponse<PendingPaymentResponse> | null>(null);
  const [history, setHistory] = useState<PagedResponse<PaymentResponse> | null>(null);
  const [page, setPage] = useState(1);

  const [selected, setSelected] = useState<PaymentResponse | null>(null);
  const [payingRentalId, setPayingRentalId] = useState<string | null>(null);

  const fetchData = async () => {
    const [pendingRes, historyRes] = await Promise.allSettled([
      paymentsApi.pending({ page: 1, pageSize: 50 }),
      paymentsApi.myPayments({ page, pageSize: 10 }),
    ]);

    setPending(pendingRes.status === 'fulfilled' ? pendingRes.value : null);
    setHistory(historyRes.status === 'fulfilled' ? historyRes.value : null);
  };

  useEffect(() => {
    fetchData();
  }, [page]);

  // Ekipmanlarım sayfasından "Şimdi Öde" ile gelindiğinde ödeme ekranını direkt aç.
  useEffect(() => {
    const payParam = searchParams.get('pay');
    if (payParam) {
      setPayingRentalId(payParam);
      searchParams.delete('pay');
      setSearchParams(searchParams, { replace: true });
    }
  }, []);

  const handlePaid = () => {
    setPayingRentalId(null);
    setPage(1);
    fetchData();
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Ödemelerim</h1>

      {/* Bekleyen ödemeler */}
      <section className="mb-8">
        <div className="flex items-center gap-2 mb-3">
          <h2 className="text-lg font-semibold text-gray-900">Bekleyen Ödemeler</h2>
          {pending && pending.totalCount > 0 && (
            <span className="bg-amber-100 text-amber-800 text-xs px-2 py-0.5 rounded-full font-medium">
              {pending.totalCount}
            </span>
          )}
        </div>

        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="text-left px-5 py-3 text-gray-500 font-medium">Ekipman</th>
                <th className="text-left px-5 py-3 text-gray-500 font-medium hidden md:table-cell">İade Tarihi</th>
                <th className="text-left px-5 py-3 text-gray-500 font-medium hidden md:table-cell">Gecikme</th>
                <th className="text-right px-5 py-3 text-gray-500 font-medium">Tutar</th>
                <th className="text-right px-5 py-3 text-gray-500 font-medium">İşlem</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {pending?.items.map((p) => (
                <tr key={p.equipmentRentalId} className="hover:bg-gray-50">
                  <td className="px-5 py-3 font-medium text-gray-900">{p.equipmentName}</td>
                  <td className="px-5 py-3 text-gray-500 hidden md:table-cell">
                    {new Date(p.releasedAt).toLocaleString('tr-TR')}
                  </td>
                  <td className="px-5 py-3 hidden md:table-cell">
                    {p.isOverdue ? (
                      <span className="text-xs px-2 py-1 rounded-full font-medium bg-red-100 text-red-800">
                        Gecikmeli
                      </span>
                    ) : (
                      <span className="text-gray-400">—</span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-right font-semibold text-gray-900">
                    {formatMoney(p.totalAmount)}
                  </td>
                  <td className="px-5 py-3 text-right">
                    <button
                      onClick={() => setPayingRentalId(p.equipmentRentalId)}
                      className="bg-blue-600 hover:bg-blue-700 text-white px-3 py-1.5 rounded text-xs font-medium"
                    >
                      Öde
                    </button>
                  </td>
                </tr>
              ))}
              {pending?.items.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-5 py-8 text-center text-gray-500">
                    Bekleyen ödemeniz yok.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      {/* Ödeme geçmişi */}
      <section>
        <h2 className="text-lg font-semibold text-gray-900 mb-3">Ödeme Geçmişi</h2>

        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="text-left px-5 py-3 text-gray-500 font-medium">Fiş No</th>
                <th className="text-left px-5 py-3 text-gray-500 font-medium hidden md:table-cell">Tarih</th>
                <th className="text-right px-5 py-3 text-gray-500 font-medium hidden md:table-cell">Kiralama</th>
                <th className="text-right px-5 py-3 text-gray-500 font-medium hidden md:table-cell">Gecikme</th>
                <th className="text-right px-5 py-3 text-gray-500 font-medium">Toplam</th>
                <th className="text-left px-5 py-3 text-gray-500 font-medium">Durum</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {history?.items.map((p) => (
                <tr
                  key={p.id}
                  onClick={() => setSelected(p)}
                  className="hover:bg-gray-50 cursor-pointer"
                >
                  <td className="px-5 py-3 font-mono text-xs text-gray-900">{p.paymentNumber}</td>
                  <td className="px-5 py-3 text-gray-500 hidden md:table-cell">
                    {new Date(p.createdAt).toLocaleString('tr-TR')}
                  </td>
                  <td className="px-5 py-3 text-right text-gray-500 hidden md:table-cell">
                    {formatMoney(p.rentalFee)}
                  </td>
                  <td className="px-5 py-3 text-right hidden md:table-cell">
                    <span className={p.lateFee > 0 ? 'text-red-600' : 'text-gray-400'}>
                      {formatMoney(p.lateFee)}
                    </span>
                  </td>
                  <td className="px-5 py-3 text-right font-semibold text-gray-900">
                    {formatMoney(p.totalAmount)}
                  </td>
                  <td className="px-5 py-3">
                    <span className={`text-xs px-2 py-1 rounded-full font-medium ${paymentStatusBadge(p.status)}`}>
                      {paymentStatusLabel(p.status)}
                    </span>
                  </td>
                </tr>
              ))}
              {history?.items.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-5 py-8 text-center text-gray-500">
                    Ödeme kaydı bulunamadı.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {history && history.totalPages > 1 && (
          <div className="flex justify-center gap-2 mt-4">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-3 py-1 rounded border text-sm disabled:opacity-50"
            >
              Önceki
            </button>
            <span className="px-3 py-1 text-sm text-gray-500">{page} / {history.totalPages}</span>
            <button
              onClick={() => setPage((p) => Math.min(history.totalPages, p + 1))}
              disabled={page === history.totalPages}
              className="px-3 py-1 rounded border text-sm disabled:opacity-50"
            >
              Sonraki
            </button>
          </div>
        )}
      </section>

      {selected && (
        <DetailModal title="Ödeme Detayı" onClose={() => setSelected(null)}>
          <DetailRow label="Fiş No" value={<span className="font-mono text-xs">{selected.paymentNumber}</span>} />
          <DetailRow
            label="Durum"
            value={paymentStatusLabel(selected.status)}
            badge={paymentStatusBadge(selected.status)}
          />
          <DetailRow label="Kiralama Ücreti" value={formatMoney(selected.rentalFee)} />
          <DetailRow label="Gecikme Ücreti" value={formatMoney(selected.lateFee)} />
          <DetailRow label="Üyelik İndirimi" value={`- ${formatMoney(selected.discountAmount)}`} />
          <DetailRow
            label="Toplam"
            value={<span className="font-semibold">{formatMoney(selected.totalAmount)}</span>}
          />
          <DetailRow label="Ödeme Yöntemi" value={selected.paymentMethod === 'Balance' ? 'Bakiye' : selected.paymentMethod} />
          <DetailRow label="Oluşturulma" value={new Date(selected.createdAt).toLocaleString('tr-TR')} />
          <DetailRow
            label="Ödenme"
            value={selected.paidAt ? new Date(selected.paidAt).toLocaleString('tr-TR') : '—'}
          />
        </DetailModal>
      )}

      {payingRentalId && (
        <PayModal
          equipmentRentalId={payingRentalId}
          onClose={() => setPayingRentalId(null)}
          onPaid={handlePaid}
        />
      )}
    </div>
  );
}
