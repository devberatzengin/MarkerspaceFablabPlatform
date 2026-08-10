import type { ReactNode } from 'react';

interface DetailModalProps {
  title: string;
  onClose: () => void;
  children: ReactNode;
}

export default function DetailModal({ title, onClose, children }: DetailModalProps) {
  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4" onClick={onClose}>
      <div
        className="bg-white rounded-lg shadow-xl w-full max-w-lg max-h-[80vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-between items-center px-6 py-4 border-b sticky top-0 bg-white rounded-t-lg">
          <h2 className="card-title mb-0">{title}</h2>
          <button onClick={onClose} className="btn-icon-small text-2xl leading-none">&times;</button>
        </div>
        <div className="px-6 py-4">
          {children}
        </div>
      </div>
    </div>
  );
}

export function DetailRow({ label, value, badge }: { label: string; value: ReactNode; badge?: string }) {
  return (
    <div className="flex justify-between items-start py-2 border-b border-gray-100 last:border-0">
      <span className="text-sm text-gray-500 shrink-0 mr-4">{label}</span>
      <span className="text-sm text-gray-900 text-right">
        {badge ? (
          <span className={`badge ${badge}`}>{value}</span>
        ) : (
          value || '-'
        )}
      </span>
    </div>
  );
}
