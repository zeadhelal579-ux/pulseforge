/**
 * File: src/shared/demo/demoText.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Every piece of server-generated text the demo backend returns, in Arabic and English.
 * Logic: A real backend returns one language; the demo localises at REQUEST time using the language currently
 *        applied to <html lang>. (DemoRuntime refetches the affected queries when the language changes.)
 * Implementation: small dictionaries plus pure builder functions; the AI narrative is deliberately
 *        viewer-aware (patients never get doctor-only details such as model flags).
 */
import type { AiReportPeriod, UserRole } from '@/shared/types/api';
import type { DemoScenario } from '@/shared/demo/demoTypes';

export type DemoLanguage = 'ar' | 'en';

interface Localized {
  en: string;
  ar: string;
}

/** Language currently shown in the UI (read from <html lang>, which I18nProvider keeps in sync). */
export function currentLanguage(): DemoLanguage {
  if (typeof document === 'undefined') return 'en';
  return document.documentElement.getAttribute('lang') === 'en' ? 'en' : 'ar';
}

/** Picks the requested language out of a localized pair. */
export function pick(text: Localized, language: DemoLanguage): string {
  return text[language];
}

// ---------------------------------------------------------------------------
// Patient-list status labels (PatientListItem.statusLabel)
// ---------------------------------------------------------------------------

export type StatusLabelKey = 'stable' | 'clinical' | 'technical' | 'critical' | 'awaiting';

const STATUS_LABELS: Record<StatusLabelKey, Localized> = {
  stable: { en: 'Fully Stable', ar: 'مستقر تمامًا' },
  clinical: { en: 'Clinical Alert', ar: 'تنبيه سريري' },
  technical: { en: 'Technical Alert', ar: 'تنبيه فني' },
  critical: { en: 'Critical Alert', ar: 'تنبيه حرج' },
  awaiting: { en: 'Awaiting Data', ar: 'في انتظار البيانات' },
};

/** Localised label shown on the doctor's patient-list status pill. */
export function statusLabelText(key: StatusLabelKey, language: DemoLanguage): string {
  return pick(STATUS_LABELS[key], language);
}

// ---------------------------------------------------------------------------
// Alerts
// ---------------------------------------------------------------------------

/** Canonical alert type strings stored in the database; localised on read. */
export const ALERT_TYPE_RISK = 'Risk Threshold';
export const ALERT_TYPE_MANUAL = 'Manual Event';

const ALERT_TYPES: Record<string, Localized> = {
  [ALERT_TYPE_RISK]: { en: 'Risk Threshold', ar: 'تجاوز حد الخطورة' },
  [ALERT_TYPE_MANUAL]: { en: 'Manual Event', ar: 'حدث يدوي' },
};

/** Localises a canonical alert type; unknown strings pass through untouched. */
export function alertTypeText(type: string, language: DemoLanguage): string {
  const entry = ALERT_TYPES[type];
  return entry ? pick(entry, language) : type;
}

export type SeedAlertKey =
  | 'activityRise'
  | 'irregularRhythm'
  | 'briefRise'
  | 'signalQuality'
  | 'criticalRaised'
  | 'escalated';

export const SEED_ALERT_TEXT: Record<SeedAlertKey, Localized> = {
  activityRise: {
    en: 'Heart rate rose above the typical range during activity and returned to baseline.',
    ar: 'نبض القلب زاد عن المعدل الطبيعي أثناء المجهود ورجع لوضعه الطبيعي.',
  },
  irregularRhythm: {
    en: 'Irregular rhythm pattern detected: status changed to Caution.',
    ar: 'اتكشف نمط نبض غير منتظم: الحالة اتغيرت لتنبيه.',
  },
  briefRise: {
    en: 'Brief heart-rate rise above the typical range.',
    ar: 'ارتفاع لحظي في النبض عن المعدل الطبيعي.',
  },
  signalQuality: {
    en: 'Signal quality dropped: electrode contact may need repositioning.',
    ar: 'جودة الإشارة قلّت: ممكن الأقطاب محتاجة تتظبط تاني.',
  },
  criticalRaised: {
    en: 'Critical status raised by the risk model: clinician review requested.',
    ar: 'نموذج تقييم الخطورة رفع الحالة لحرجة: مطلوب مراجعة الطبيب.',
  },
  escalated: {
    en: 'Status moved from Stable to Caution: heart rate above the typical range.',
    ar: 'الحالة اتحولت من مستقر لتنبيه: النبض أعلى من المعدل الطبيعي.',
  },
};

export const SEED_RESOLUTION_NOTE: Localized = {
  en: 'Auto-resolved: status returned to Stable.',
  ar: 'اتحل تلقائيًا: الحالة رجعت مستقرة.',
};

export const SEED_ESCALATION_NOTE: Localized = {
  en: 'Escalated to Critical.',
  ar: 'اتصعّد لحالة حرجة.',
};

// ---------------------------------------------------------------------------
// AI narrative (GET /patients/{id|me}/ai-report)
// ---------------------------------------------------------------------------

const PERIOD_LABEL: Record<AiReportPeriod, Localized> = {
  daily: { en: 'past 24 hours', ar: 'آخر 24 ساعة' },
  weekly: { en: 'past 7 days', ar: 'آخر 7 أيام' },
  monthly: { en: 'past 30 days', ar: 'آخر 30 يوم' },
};

const DEMO_SUFFIX: Localized = {
  en: ' (Generated from simulated demo data.)',
  ar: ' (متولّد من بيانات تجريبية محاكاة.)',
};

export interface NarrativeInput {
  scenario: Exclude<DemoScenario, 'idle'>;
  technicalAlert: boolean;
  period: AiReportPeriod;
  averageHeartRate: number;
  alertCount: number;
  viewer: UserRole;
  language: DemoLanguage;
}

/** Sentence about how many alerts were logged in the period. */
function alertSentence(count: number, language: DemoLanguage): string {
  if (language === 'ar') {
    return count === 0 ? 'مفيش تنبيهات اتسجلت في الفترة دي.' : `عدد التنبيهات المسجلة: ${count}.`;
  }
  if (count === 0) return 'No alerts were logged in this period.';
  return `${count} alert${count === 1 ? ' was' : 's were'} logged.`;
}

/** Builds the demo "AI-assisted" narrative. Patients get plain language only (no model flags or scores). */
export function buildNarrative(input: NarrativeInput): string {
  const { scenario, technicalAlert, period, averageHeartRate: avg, viewer, language } = input;
  const span = pick(PERIOD_LABEL[period], language);
  const alerts = alertSentence(input.alertCount, language);
  const isDoctor = viewer === 'Doctor';
  let body: string;

  if (language === 'ar') {
    if (scenario === 'stable') {
      body = `خلال ${span} فضلت القراءات ضمن المعدل الطبيعي (متوسط النبض ${avg} نبضة/د). ${alerts} مفيش إجراء مطلوب غير المتابعة الروتينية.`;
    } else if (scenario === 'caution' && technicalAlert) {
      body = `خلال ${span} كان متوسط النبض ${avg} نبضة/د، لكن جودة الإشارة قلّت في بعض الأوقات لأن الأقطاب ممكن تحتاج تتظبط تاني. ${alerts} راجع مكان الأقطاب وتلامسها مع الجلد.`;
    } else if (scenario === 'caution') {
      body = `خلال ${span} كان متوسط النبض ${avg} نبضة/د مع فترات نبض غير منتظم وقراءات أعلى من المعدل الطبيعي. ${alerts} ${
        isDoctor ? 'يُنصح بمراجعة نمط النبض إكلينيكيًا.' : 'فريق الرعاية اتبلّغ وممكن يتواصل معاك.'
      }`;
    } else if (isDoctor) {
      body = `خلال ${span} فضلت القراءات خارج المعدل الطبيعي (متوسط النبض ${avg} نبضة/د) والنموذج أشّر لتغيرات محتملة في مقطع ST. ${alerts} يُنصح بمراجعة عاجلة من الطبيب.`;
    } else {
      body = `خلال ${span} كانت قراءاتك خارج المعدل الطبيعي (متوسط النبض ${avg} نبضة/د). فريق الرعاية اتبلّغ. لو حاسس بتعب اتصل بطبيبك أو بالإسعاف فورًا.`;
    }
  } else if (scenario === 'stable') {
    body = `Over the ${span}, readings stayed within the typical range (average heart rate ${avg} bpm). ${alerts} No action is suggested beyond routine follow-up.`;
  } else if (scenario === 'caution' && technicalAlert) {
    body = `Over the ${span}, the average heart rate was ${avg} bpm, but signal quality dropped at times because electrode contact may need repositioning. ${alerts} Check the electrode placement and skin contact.`;
  } else if (scenario === 'caution') {
    body = `Over the ${span}, the average heart rate was ${avg} bpm with periods of irregular rhythm and readings above the typical range. ${alerts} ${
      isDoctor
        ? 'A clinical review of the rhythm pattern is suggested.'
        : 'Your care team has been notified and may follow up with you.'
    }`;
  } else if (isDoctor) {
    body = `Over the ${span}, readings stayed outside the typical range (average heart rate ${avg} bpm) and the model flagged possible ST-segment changes. ${alerts} Urgent clinician review is recommended.`;
  } else {
    body = `Over the ${span}, your readings have been outside the typical range (average heart rate ${avg} bpm). Your care team has been alerted. If you feel unwell, contact your doctor or emergency services right away.`;
  }

  return body + pick(DEMO_SUFFIX, language);
}
