import { useEffect, useMemo, useState } from 'react';
import { announcementsApi } from '../api/announcements';
import { notificationsApi } from '../api/notifications';
import { equipmentApi } from '../api/equipment';
import { paymentsApi } from '../api/payments';

type ActivityKind =
  | 'announcement'
  | 'published'
  | 'notification'
  | 'rental'
  | 'return'
  | 'due'
  | 'payment';

interface ActivityEvent {
  id: string;
  kind: ActivityKind;
  at: Date;
  title: string;
  detail?: string;
}

// Tailwind sınıfları statik olmalı (JIT dinamik string'leri göremez), o yüzden tam sınıf adları.
const kindConfig: Record<ActivityKind, { label: string; dot: string; chip: string; icon: string }> = {
  announcement: { label: 'Duyuru oluşturuldu', dot: 'bg-slate-400', chip: 'bg-slate-100 text-slate-700', icon: '📝' },
  published: { label: 'Duyuru yayınlandı', dot: 'bg-blue-500', chip: 'bg-blue-100 text-blue-700', icon: '📣' },
  notification: { label: 'Bildirim', dot: 'bg-purple-500', chip: 'bg-purple-100 text-purple-700', icon: '🔔' },
  rental: { label: 'Ekipman alındı', dot: 'bg-amber-500', chip: 'bg-amber-100 text-amber-700', icon: '🔧' },
  return: { label: 'Ekipman iade edildi', dot: 'bg-green-500', chip: 'bg-green-100 text-green-700', icon: '✅' },
  due: { label: 'Planlanan iade', dot: 'bg-red-500', chip: 'bg-red-100 text-red-700', icon: '⏰' },
  payment: { label: 'Ödeme', dot: 'bg-teal-500', chip: 'bg-teal-100 text-teal-700', icon: '💳' },
};

const allKinds = Object.keys(kindConfig) as ActivityKind[];

const weekDays = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];
const monthNames = [
  'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
  'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık',
];

/** Yerel saate göre "YYYY-MM-DD". Gün gruplaması UTC'ye göre yapılırsa gece yarısı civarı kaymalar olur. */
const dayKey = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

/** Pazartesi ile başlayan haftada, verilen günün 0-6 arası sırası. */
const mondayIndex = (d: Date) => (d.getDay() + 6) % 7;

const timeLabel = (d: Date) =>
  d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });

const currencyLabel = (amount: number) =>
  amount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 2 });

export default function ActivityCalendar() {
  const [events, setEvents] = useState<ActivityEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [cursor, setCursor] = useState(() => new Date());
  const [selectedKey, setSelectedKey] = useState(() => dayKey(new Date()));
  const [hidden, setHidden] = useState<Set<ActivityKind>>(new Set());

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      // allSettled: bir uç hata verirse (ör. yetki) takvimin tamamı çökmesin.
      const [announcements, notifications, rentals, payments] = await Promise.allSettled([
        announcementsApi.getAll({ page: 1, pageSize: 100 }),
        notificationsApi.getMine({ page: 1, pageSize: 100 }),
        equipmentApi.myEquipments({ page: 1, pageSize: 100, includePast: true }),
        paymentsApi.myPayments({ page: 1, pageSize: 100 }),
      ]);

      if (cancelled) return;

      const collected: ActivityEvent[] = [];

      if (announcements.status === 'fulfilled') {
        for (const a of announcements.value.items) {
          const createdAt = new Date(a.createdAt);
          const updatedAt = new Date(a.updatedAt);

          collected.push({
            id: `announcement-${a.id}`,
            kind: 'announcement',
            at: createdAt,
            title: a.title,
            detail: `${a.categoryName} · ${a.createdByName}`,
          });

          // Yayınlanma anı için ayrı bir kolon yok; Published duyurularda son güncelleme
          // anını yayın anı olarak gösteriyoruz (oluşturma ile aynı dakikaysa tekrar etmesin).
          if (a.status === 'Published' && updatedAt.getTime() - createdAt.getTime() > 60_000) {
            collected.push({
              id: `published-${a.id}`,
              kind: 'published',
              at: updatedAt,
              title: a.title,
              detail: 'Yayına alındı',
            });
          }
        }
      }

      if (notifications.status === 'fulfilled') {
        for (const n of notifications.value.items) {
          collected.push({
            id: `notification-${n.id}`,
            kind: 'notification',
            at: new Date(n.createdAt),
            title: n.title,
            detail: n.message,
          });
        }
      }

      if (rentals.status === 'fulfilled') {
        for (const r of rentals.value.items) {
          collected.push({
            id: `rental-${r.id}`,
            kind: 'rental',
            at: new Date(r.rentedAt),
            title: r.equipmentName,
            detail: 'Kiralama başladı',
          });

          if (r.releasedAt) {
            const releasedAt = new Date(r.releasedAt);
            const late = releasedAt.getTime() > new Date(r.expectedReturnAt).getTime();

            collected.push({
              id: `return-${r.id}`,
              kind: 'return',
              at: releasedAt,
              title: r.equipmentName,
              detail: late ? 'İade edildi (gecikmeli)' : 'İade edildi',
            });
          } else {
            collected.push({
              id: `due-${r.id}`,
              kind: 'due',
              at: new Date(r.expectedReturnAt),
              title: r.equipmentName,
              detail: 'Planlanan iade tarihi',
            });
          }
        }
      }

      if (payments.status === 'fulfilled') {
        for (const p of payments.value.items) {
          collected.push({
            id: `payment-${p.id}`,
            kind: 'payment',
            at: new Date(p.paidAt ?? p.createdAt),
            title: currencyLabel(p.totalAmount),
            detail: p.paymentNumber,
          });
        }
      }

      collected.sort((a, b) => a.at.getTime() - b.at.getTime());
      setEvents(collected);
      setLoading(false);
    };

    load().catch(() => !cancelled && setLoading(false));

    return () => {
      cancelled = true;
    };
  }, []);

  const visibleEvents = useMemo(
    () => events.filter((e) => !hidden.has(e.kind)),
    [events, hidden],
  );

  /** Gün anahtarı -> o güne düşen olaylar. */
  const eventsByDay = useMemo(() => {
    const map = new Map<string, ActivityEvent[]>();
    for (const event of visibleEvents) {
      const key = dayKey(event.at);
      const bucket = map.get(key);
      if (bucket) bucket.push(event);
      else map.set(key, [event]);
    }
    return map;
  }, [visibleEvents]);

  /** Ayın ilk gününü içeren pazartesiden başlayan 42 hücrelik ızgara. */
  const cells = useMemo(() => {
    const first = new Date(cursor.getFullYear(), cursor.getMonth(), 1);
    const gridStart = new Date(first);
    gridStart.setDate(first.getDate() - mondayIndex(first));

    return Array.from({ length: 42 }, (_, i) => {
      const date = new Date(gridStart);
      date.setDate(gridStart.getDate() + i);
      return date;
    });
  }, [cursor]);

  const todayKey = dayKey(new Date());
  const selectedEvents = eventsByDay.get(selectedKey) ?? [];
  const selectedDate = useMemo(() => {
    const [year, month, day] = selectedKey.split('-').map(Number);
    return new Date(year, month - 1, day);
  }, [selectedKey]);

  const monthEventCount = useMemo(
    () =>
      visibleEvents.filter(
        (e) => e.at.getFullYear() === cursor.getFullYear() && e.at.getMonth() === cursor.getMonth(),
      ).length,
    [visibleEvents, cursor],
  );

  const shiftMonth = (delta: number) =>
    setCursor((prev) => new Date(prev.getFullYear(), prev.getMonth() + delta, 1));

  const goToday = () => {
    const now = new Date();
    setCursor(new Date(now.getFullYear(), now.getMonth(), 1));
    setSelectedKey(dayKey(now));
  };

  const toggleKind = (kind: ActivityKind) =>
    setHidden((prev) => {
      const next = new Set(prev);
      if (next.has(kind)) next.delete(kind);
      else next.add(kind);
      return next;
    });

  return (
    <div className="bg-white rounded-lg shadow">
      <div className="px-5 py-4 border-b flex flex-wrap justify-between items-center gap-3">
        <div>
          <h2 className="font-semibold text-gray-900">Etkinlik Takvimi</h2>
          <p className="text-xs text-gray-500 mt-0.5">
            {loading ? 'Yükleniyor...' : `Bu ayda ${monthEventCount} kayıt`}
          </p>
        </div>

        <div className="flex items-center gap-1">
          <button
            onClick={() => shiftMonth(-1)}
            aria-label="Önceki ay"
            className="w-8 h-8 rounded-md border border-gray-300 text-gray-600 hover:bg-gray-50 text-sm"
          >
            ‹
          </button>
          <span className="px-2 text-sm font-medium text-gray-900 min-w-[120px] text-center">
            {monthNames[cursor.getMonth()]} {cursor.getFullYear()}
          </span>
          <button
            onClick={() => shiftMonth(1)}
            aria-label="Sonraki ay"
            className="w-8 h-8 rounded-md border border-gray-300 text-gray-600 hover:bg-gray-50 text-sm"
          >
            ›
          </button>
          <button
            onClick={goToday}
            className="ml-2 px-3 h-8 rounded-md border border-gray-300 text-gray-600 hover:bg-gray-50 text-xs"
          >
            Bugün
          </button>
        </div>
      </div>

      {/* Legend / filtre */}
      <div className="px-5 py-3 border-b flex flex-wrap gap-2">
        {allKinds.map((kind) => {
          const isHidden = hidden.has(kind);
          return (
            <button
              key={kind}
              onClick={() => toggleKind(kind)}
              title={isHidden ? 'Göster' : 'Gizle'}
              className={`flex items-center gap-1.5 px-2 py-1 rounded-full text-xs border transition-colors ${
                isHidden
                  ? 'border-gray-200 text-gray-400 bg-white'
                  : 'border-transparent ' + kindConfig[kind].chip
              }`}
            >
              <span className={`w-2 h-2 rounded-full ${isHidden ? 'bg-gray-300' : kindConfig[kind].dot}`} />
              {kindConfig[kind].label}
            </button>
          );
        })}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3">
        {/* Ay ızgarası */}
        <div className="lg:col-span-2 p-4 lg:border-r">
          <div className="grid grid-cols-7 gap-1 mb-1">
            {weekDays.map((d) => (
              <div key={d} className="text-center text-xs font-medium text-gray-400 py-1">
                {d}
              </div>
            ))}
          </div>

          <div className="grid grid-cols-7 gap-1">
            {cells.map((date) => {
              const key = dayKey(date);
              const dayEvents = eventsByDay.get(key) ?? [];
              const inMonth = date.getMonth() === cursor.getMonth();
              const isToday = key === todayKey;
              const isSelected = key === selectedKey;

              return (
                <button
                  key={key}
                  onClick={() => setSelectedKey(key)}
                  className={`min-h-[62px] rounded-md border p-1.5 text-left flex flex-col transition-colors ${
                    isSelected
                      ? 'border-blue-500 bg-blue-50'
                      : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                  } ${inMonth ? '' : 'opacity-40'}`}
                >
                  <span
                    className={`text-xs font-medium ${
                      isToday
                        ? 'bg-blue-600 text-white rounded-full w-5 h-5 flex items-center justify-center'
                        : 'text-gray-700'
                    }`}
                  >
                    {date.getDate()}
                  </span>

                  <span className="flex flex-wrap gap-0.5 mt-auto pt-1">
                    {dayEvents.slice(0, 4).map((e) => (
                      <span
                        key={e.id}
                        title={`${kindConfig[e.kind].label}: ${e.title}`}
                        className={`w-1.5 h-1.5 rounded-full ${kindConfig[e.kind].dot}`}
                      />
                    ))}
                    {dayEvents.length > 4 && (
                      <span className="text-[10px] leading-none text-gray-400">
                        +{dayEvents.length - 4}
                      </span>
                    )}
                  </span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Seçili günün detayı */}
        <div className="p-4 lg:max-h-[520px] lg:overflow-y-auto">
          <h3 className="text-sm font-semibold text-gray-900 mb-1">
            {selectedDate.toLocaleDateString('tr-TR', {
              day: 'numeric',
              month: 'long',
              year: 'numeric',
              weekday: 'long',
            })}
          </h3>
          <p className="text-xs text-gray-500 mb-3">
            {selectedEvents.length === 0 ? 'Kayıt yok' : `${selectedEvents.length} kayıt`}
          </p>

          {selectedEvents.length === 0 ? (
            <div className="text-center text-gray-400 text-sm py-10 border border-dashed border-gray-200 rounded-md">
              Bu güne ait bir hareket yok.
            </div>
          ) : (
            <ul className="space-y-2">
              {selectedEvents.map((event) => (
                <li key={event.id} className="border border-gray-200 rounded-md p-3">
                  <div className="flex items-start gap-2">
                    <span className="text-base leading-none mt-0.5">{kindConfig[event.kind].icon}</span>
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className={`text-[10px] px-1.5 py-0.5 rounded-full ${kindConfig[event.kind].chip}`}>
                          {kindConfig[event.kind].label}
                        </span>
                        <span className="text-xs text-gray-400">{timeLabel(event.at)}</span>
                      </div>
                      <p className="text-sm font-medium text-gray-900 mt-1 break-words">{event.title}</p>
                      {event.detail && (
                        <p className="text-xs text-gray-500 mt-0.5 break-words">{event.detail}</p>
                      )}
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}
