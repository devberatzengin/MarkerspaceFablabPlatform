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
  const [preview, setPreview] = useState<PendingPaymentResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Tutar her zaman backend'den gelir; frontend hesaplama yapmaz.
  useEffect(() => {
    let cancelled = false;

    paymentsApi
      .preview(equipmentRentalId)
      .then((data) => {
        if (!cancelled) setPreview(data);
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

      {!loading && !preview && (
        <p className="error-banner">{error ?? 'Tutar bilgisi alınamadı.'}</p>
      )}

      {preview && (
        <>
          <DetailRow label="Ekipman" value={preview.equipmentName} />
          <DetailRow label="Kiralama" value={new Date(preview.rentedAt).toLocaleString('tr-TR')} />
          <DetailRow label="İade" value={new Date(preview.releasedAt).toLocaleString('tr-TR')} />

          <div className="mt-4 pt-3 border-t">
            <DetailRow label="Kiralama Ücreti" value={formatMoney(preview.rentalFee)} />
            <DetailRow
              label="Gecikme Ücreti"
              value={
                <span className={preview.lateFee > 0 ? 'text-red-600 font-medium' : undefined}>
                  {formatMoney(preview.lateFee)}
                </span>
              }
            />
            <DetailRow
              label="Üyelik İndirimi"
              value={<span className="text-green-700">- {formatMoney(preview.discountAmount)}</span>}
            />
          </div>

          <div className="flex justify-between items-center mt-3 pt-3 border-t">
            <span className="text-sm font-medium text-gray-700">Ödenecek Tutar</span>
            <span className="text-xl font-bold text-gray-900">{formatMoney(preview.totalAmount)}</span>
          </div>

          <div className="flex justify-between items-center mt-2 text-sm">
            <span className="text-gray-500">Bakiyeniz</span>
            <span className={preview.hasSufficientBalance ? 'text-gray-700' : 'text-red-600 font-medium'}>
              {formatMoney(preview.userBalance)}
            </span>
          </div>

          {!preview.hasSufficientBalance && (
            <p className="error-banner mt-3">
              Bakiyeniz yetersiz. Profil sayfanızdan bakiye yükleyip tekrar deneyin.
            </p>
          )}

          {error && (
            <p className="error-banner mt-3">{error}</p>
          )}

          <div className="flex justify-end gap-2 mt-5">
            <button
              onClick={onClose}
              disabled={submitting}
              className="btn-secondary"
            >
              Vazgeç
            </button>
            <button
              onClick={handlePay}
              disabled={submitting || !preview.hasSufficientBalance}
              className="btn-primary"
            >
              {submitting ? 'İşleniyor…' : `${formatMoney(preview.totalAmount)} Öde`}
            </button>
          </div>
        </>
      )}
    </DetailModal>
  );
}
