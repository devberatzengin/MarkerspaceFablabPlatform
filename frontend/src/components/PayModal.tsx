import { useEffect, useState } from 'react';
import { paymentsApi } from '../api/payments';
import DetailModal, { DetailRow } from './DetailModal';
import { errorMessage, formatMoney } from '../utils/payment';
import type { PendingPaymentResponse } from '../types';

interface PayModalProps {
  equipmentRentalId: string;
  onClose: () => void;
  onPaid: () => void;
}

export default function PayModal({ equipmentRentalId, onClose, onPaid }: PayModalProps) {
  const [quote, setQuote] = useState<PendingPaymentResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Tutar her zaman backend'den gelir; frontend hesaplama yapmaz.
  useEffect(() => {
    let cancelled = false;

    paymentsApi
      .quote(equipmentRentalId)
      .then((data) => {
        if (!cancelled) setQuote(data);
      })
      .catch((err) => {
        if (!cancelled) setError(errorMessage(err, 'Tutar hesaplanamadı.'));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [equipmentRentalId]);

  const handlePay = async () => {
    setSubmitting(true);
    setError(null);
    try {
      await paymentsApi.create({ equipmentRentalId });
      onPaid();
    } catch (err) {
      setError(errorMessage(err, 'Ödeme alınamadı.'));
      setSubmitting(false);
    }
  };

  return (
    <DetailModal title="Ödeme" onClose={onClose}>
      {loading && <p className="text-sm text-gray-500 py-4">Tutar hesaplanıyor…</p>}

      {!loading && !quote && (
        <p className="text-sm text-red-600 py-4">{error ?? 'Tutar bilgisi alınamadı.'}</p>
      )}

      {quote && (
        <>
          <DetailRow label="Ekipman" value={quote.equipmentName} />
          <DetailRow label="Kiralama" value={new Date(quote.rentedAt).toLocaleString('tr-TR')} />
          <DetailRow label="İade" value={new Date(quote.releasedAt).toLocaleString('tr-TR')} />

          <div className="mt-4 pt-3 border-t">
            <DetailRow label="Kiralama Ücreti" value={formatMoney(quote.rentalFee)} />
            <DetailRow
              label="Gecikme Ücreti"
              value={
                <span className={quote.lateFee > 0 ? 'text-red-600 font-medium' : undefined}>
                  {formatMoney(quote.lateFee)}
                </span>
              }
            />
            <DetailRow
              label="Üyelik İndirimi"
              value={<span className="text-green-700">- {formatMoney(quote.discountAmount)}</span>}
            />
          </div>

          <div className="flex justify-between items-center mt-3 pt-3 border-t">
            <span className="text-sm font-medium text-gray-700">Ödenecek Tutar</span>
            <span className="text-xl font-bold text-gray-900">{formatMoney(quote.totalAmount)}</span>
          </div>

          <div className="flex justify-between items-center mt-2 text-sm">
            <span className="text-gray-500">Bakiyeniz</span>
            <span className={quote.hasSufficientBalance ? 'text-gray-700' : 'text-red-600 font-medium'}>
              {formatMoney(quote.userBalance)}
            </span>
          </div>

          {!quote.hasSufficientBalance && (
            <p className="mt-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded p-3">
              Bakiyeniz yetersiz. Profil sayfanızdan bakiye yükleyip tekrar deneyin.
            </p>
          )}

          {error && (
            <p className="mt-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded p-3">{error}</p>
          )}

          <div className="flex justify-end gap-2 mt-5">
            <button
              onClick={onClose}
              disabled={submitting}
              className="px-4 py-2 rounded border text-sm text-gray-700 hover:bg-gray-50 disabled:opacity-50"
            >
              Vazgeç
            </button>
            <button
              onClick={handlePay}
              disabled={submitting || !quote.hasSufficientBalance}
              className="px-4 py-2 rounded bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium disabled:opacity-50"
            >
              {submitting ? 'İşleniyor…' : `${formatMoney(quote.totalAmount)} Öde`}
            </button>
          </div>
        </>
      )}
    </DetailModal>
  );
}
