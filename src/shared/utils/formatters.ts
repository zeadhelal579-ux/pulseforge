type AppLocale = 'ar' | 'en';

/**
 * Arabic locales default to Arabic-Indic digits (٠١٢) in some browsers/OSes.
 * Appending `-u-nu-latn` forces Latin digits (0123) so BPM, mmHg, ms, serials,
 * etc. read identically regardless of language — matching the existing
 * dashboards' behavior described in frontend-architecture.md §9.
 */
function toIntlLocale(locale: AppLocale): string {
  return locale === 'ar' ? 'ar-EG-u-nu-latn' : 'en-US';
}

/** "74" → "74 BPM" / "74 نبضة/د" */
export function formatBpm(value: number, locale: AppLocale): string {
  const number = new Intl.NumberFormat(toIntlLocale(locale)).format(value);
  return locale === 'ar' ? `${number} نبضة/د` : `${number} BPM`;
}

/** "98" → "98%" (used for SpO2, battery level, confidence, etc.) */
export function formatPercent(value: number, locale: AppLocale, fractionDigits = 0): string {
  const number = new Intl.NumberFormat(toIntlLocale(locale), {
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits,
  }).format(value);
  return `${number}%`;
}

/** "152" → "152 ms" — used for PR interval, QRS duration, QT/QTc. */
export function formatMs(value: number, locale: AppLocale): string {
  const number = new Intl.NumberFormat(toIntlLocale(locale)).format(value);
  return locale === 'ar' ? `${number} مللي ثانية` : `${number} ms`;
}

/** Full timestamp, e.g. "Sep 13, 2026, 08:00" / "13 سبتمبر 2026، 08:00" */
export function formatTimestamp(iso: string, locale: AppLocale): string {
  const date = new Date(iso);
  return new Intl.DateTimeFormat(toIntlLocale(locale), {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date);
}

/** Just the clock time, e.g. "08:00" — used in the hourly status timeline. */
export function formatHour(hour: number, locale: AppLocale): string {
  const date = new Date();
  date.setHours(hour, 0, 0, 0);
  return new Intl.DateTimeFormat(toIntlLocale(locale), {
    hour: '2-digit',
    hour12: false,
  }).format(date);
}

/** "6 min ago" / "منذ 6 دقائق" — used on alert timeline entries and the patient list. */
export function formatRelativeTime(iso: string, locale: AppLocale): string {
  const rtf = new Intl.RelativeTimeFormat(toIntlLocale(locale), { numeric: 'auto' });
  const diffMs = new Date(iso).getTime() - Date.now();
  const diffMinutes = Math.round(diffMs / 60_000);

  if (Math.abs(diffMinutes) < 60) return rtf.format(diffMinutes, 'minute');
  const diffHours = Math.round(diffMinutes / 60);
  if (Math.abs(diffHours) < 24) return rtf.format(diffHours, 'hour');
  const diffDays = Math.round(diffHours / 24);
  return rtf.format(diffDays, 'day');
}

/** "-62" → "-62 dBm" — device signal strength on the telemetry card. */
export function formatSignalStrength(dbm: number): string {
  return `${dbm} dBm`;
}
