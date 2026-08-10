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
      <h1 className="page-title">Ödemelerim</h1>

      {/* Bekleyen ödemeler */}
      <section className="mb-8">
        <div className="flex items-center gap-2 mb-3">
          <h2 className="section-title mb-0">Bekleyen Ödemeler</h2>
          {pending && pending.totalCount > 0 && (
            <span className="badge badge-pending">
              {pending.totalCount}
            </span>
          )}
        </div>

        <div className="card p-0 overflow-hidden">
          <table className="table">
            <thead>
              <tr>
                <th>Ekipman</th>
                <th className="hidden md:table-cell">İade Tarihi</th>
                <th className="hidden md:table-cell">Gecikme</th>
                <th className="text-right">Tutar</th>
                <th className="text-right">İşlem</th>
              </tr>
            </thead>
            <tbody>
              {pending?.items.map((p) => (
                <tr key={p.equipmentRentalId}>
                  <td className="font-medium text-gray-900">{p.equipmentName}</td>
                  <td className="hidden md:table-cell text-gray-500">
                    {new Date(p.releasedAt).toLocaleString('tr-TR')}
                  </td>
                  <td className="hidden md:table-cell">
                    {p.isOverdue ? (
                      <span className="badge badge-failed">
                        Gecikmeli
                      </span>
                    ) : (
                      <span className="text-gray-400">—</span>
                    )}
                  </td>
                  <td className="text-right font-semibold text-gray-900">
                    {formatMoney(p.totalAmount)}
                  </td>
                  <td className="text-right">
                    <button
                      onClick={() => setPayingRentalId(p.equipmentRentalId)}
                      className="btn-primary btn-sm"
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
        <h2 className="section-title">Ödeme Geçmişi</h2>

        <div className="card p-0 overflow-hidden">
          <table className="table">
            <thead>
              <tr>
                <th>Fiş No</th>
                <th className="hidden md:table-cell">Tarih</th>
                <th className="text-right hidden md:table-cell">Kiralama</th>
                <th className="text-right hidden md:table-cell">Gecikme</th>
                <th className="text-right">Toplam</th>
                <th>Durum</th>
              </tr>
            </thead>
            <tbody>
              {history?.items.map((p) => (
                <tr
                  key={p.id}
                  onClick={() => setSelected(p)}
                  className="cursor-pointer"
                >
                  <td className="font-mono text-xs text-gray-900">{p.paymentNumber}</td>
                  <td className="hidden md:table-cell text-gray-500">
                    {new Date(p.createdAt).toLocaleString('tr-TR')}
                  </td>
                  <td className="text-right hidden md:table-cell text-gray-500">
                    {formatMoney(p.rentalFee)}
                  </td>
                  <td className="text-right hidden md:table-cell">
                    <span className={p.lateFee > 0 ? 'text-red-600' : 'text-gray-400'}>
                      {formatMoney(p.lateFee)}
                    </span>
                  </td>
                  <td className="text-right font-semibold text-gray-900">
                    {formatMoney(p.totalAmount)}
                  </td>
                  <td>
                    <span className={`badge ${paymentStatusBadge(p.status)}`}>
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
              className="btn-secondary disabled:opacity-50"
            >
              Önceki
            </button>
            <span className="px-3 py-1 text-sm text-gray-500">{page} / {history.totalPages}</span>
            <button
              onClick={() => setPage((p) => Math.min(history.totalPages, p + 1))}
              disabled={page === history.totalPages}
              className="btn-secondary disabled:opacity-50"
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
