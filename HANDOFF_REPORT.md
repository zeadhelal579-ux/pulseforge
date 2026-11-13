# تقرير تسليم الفرونت إند — CardioSense Web Client
**تاريخ:** 16 سبتمبر 2026
**الغرض من الملف ده:** شرح كامل لكل حاجة اتبنت في الفرونت إند، إزاي كل ملف شغال ولية اتعمل بالشكل ده، وأهم حاجة: كل نقطة محتاجة قرار أو تأكيد من حد الباك إند عشان الـ endpoints تتظبط 100%.

---

## 1. ملخص سريع

اتبنى فرونت إند React 19 + TypeScript + Vite + Tailwind v4 كامل، ماشي بالظبط على البنية الموجودة في `frontend-architecture.md`، وبيغطي:

- صفحة تسجيل الدخول/إنشاء حساب (بديل React كامل لملف `cardiosense_auth.html` الأصلي، بنفس الشكل والتفاعل بالظبط)
- **لوحة المريض** (Patient Dashboard) — نفس المكونات المستخدمة في لوحة الطبيب، حسب قرار "component واحد، chrome مختلف حسب الدور" الموثق في `PROJECT_OVERVIEW.md` §4
- **لوحة الطبيب**: قائمة المرضى (بحث + فلاتر + إحصائيات) وصفحة تفاصيل مريض واحد (بنفس مكونات لوحة المريض + أدوات الطبيب: Log Manual Event, Freeze Waveform, Call, Send Message)
- صفحة الإعدادات (لغة، وضع ليلي/نهاري، ربط الجهاز)
- كل الـ infrastructure: Axios client مع تجديد التوكن التلقائي، SignalR provider للبيانات الحية، i18n كامل (عربي/إنجليزي) مع RTL/LTR حقيقي، Zustand stores، TanStack Query

**العدد الكلي: 108 ملف.** الشجرة الكاملة في القسم 3.

**أهم حاجة قبل ما تكمل قراءة:** روح على **القسم 6 (الفجوات المطلوب تأكيدها من الباك إند)** الأول — فيه حوالي 8 نقاط لازم قرار منك أو من الباك إند قبل ما تبدأ تربط الـ API فعليًا، عشان الشكل بتاع الـ response يتظبط من أول مرة.

---

## 2. إزاي شغال المشروع (Quickstart)

```bash
cd CardioSense-Frontend
npm install
cp .env.example .env   # حط فيه رابط الـ API والـ SignalR hub الحقيقيين
npm run dev
```

تفاصيل كل الـ scripts والمتغيرات البيئية موجودة في `README.md` جوه المشروع.

**ملحوظة مهمة:** الـ sandbox اللي بنيت فيه المشروع مفيهوش إنترنت (`npm install` مش شغال هنا)، فمقدرتش أعمل `npm run build` أو `npm run test` حقيقي. اللي عملته بدل كده موضح بالتفصيل في القسم 7 (التحقق والاختبار) — استخدمت الـ TypeScript compiler الحقيقي (نسخة تانية متاحة في الـ sandbox) مع stub files مؤقتة عشان أعمل فحص حقيقي لجزء كبير من الكود، بالإضافة لسكريبتات فحص مكتوبة خصيصًا. أول حاجة تعملها إنت: `npm install && npm run build && npm run test` عشان تتأكد إن كل حاجة شغالة 100% في بيئة حقيقية.

---

## 3. شجرة المشروع الكاملة

```
CardioSense-Frontend/
├── README.md                          # دليل سريع للمشروع + الـ scripts
├── HANDOFF_REPORT.md                  # الملف ده
├── package.json                       # كل الـ dependencies (تفصيل في §4.1)
├── vite.config.ts                     # إعدادات Vite + alias @/ + proxy محلي لـ /api و /hubs
├── vitest.config.ts                   # إعدادات الاختبارات (منفصلة عن vite.config.ts عمدًا)
├── tsconfig.json / tsconfig.node.json # TypeScript config (strict mode كامل)
├── postcss.config.js                  # Tailwind v4 عبر PostCSS
├── .eslintrc.cjs / .prettierrc        # قواعد الكود
├── .env.example                       # نموذج المتغيرات البيئية (مفيش .env حقيقي متعمول commit)
├── index.html                         # نقطة الدخول، بيحمّل خطوط Inter/Tajawal/JetBrains Mono
├── public/
│   ├── favicon.svg                    # أيقونة القلب المأخوذة من صفحة اللوجن
│   └── brand/pulseforge-logo.jpeg     # لوجو PulseForge (الهاردوير) — مش مستخدم في الواجهة حاليًا، محفوظ للاستخدام المستقبلي لو حبيت تحط "Powered by PulseForge" في مكان ما
│
└── src/
    ├── main.tsx                      # نقطة الدخول لـ React
    ├── vite-env.d.ts                 # types بتاعة import.meta.env
    │
    ├── app/                          # composition root — تجميع الـ providers والـ routes، مفيهوش business logic
    │   ├── App.tsx                   # ترتيب الـ providers (مهم جدًا الترتيب، موضح في الكومنت جوه الملف)
    │   ├── router.tsx                # شجرة الـ routes كاملة
    │   ├── layouts/
    │   │   ├── AuthLayout.tsx        # شكل صفحة اللوجن (hero + card)
    │   │   ├── DoctorLayout.tsx      # sidebar + topbar بتاع الطبيب
    │   │   └── PatientLayout.tsx     # topbar بسيط بتاع المريض
    │   └── providers/
    │       ├── i18n.ts               # إعداد i18next instance
    │       ├── I18nProvider.tsx      # بيربط dir/lang في الـ <html> مع اللغة الحالية
    │       ├── QueryProvider.tsx     # TanStack Query
    │       ├── AuthProvider.tsx      # bootstrap الجلسة + تجديد اللغة على الباك إند (تفصيل §4.2)
    │       └── SignalRProvider.tsx   # بيفتح اتصال SignalR واحد للتطبيق كله
    │
    ├── features/                     # فولدر لكل capability — تفصيل كامل في §4
    │   ├── auth/                     # تسجيل دخول/حساب
    │   ├── patient-dashboard/        # كل مكونات شاشة تفاصيل المريض (مشتركة بين الدورين)
    │   ├── doctor-dashboard/         # قائمة المرضى + الأدوات الخاصة بالطبيب فقط
    │   └── settings/                 # الإعدادات
    │
    ├── shared/                       # كود مشترك — ممنوع يستورد من أي feature (اتجاه واحد بس)
    │   ├── types/api.ts              # كل الـ DTOs اللي بتحاكي الباك إند — أهم ملف في المشروع (§5)
    │   ├── lib/                      # apiClient (axios) / signalrClient / queryClient
    │   ├── store/themeStore.ts       # الوضع الليلي/النهاري
    │   ├── hooks/                    # useSignalR / useDirection / useDebouncedValue / ...
    │   ├── components/ui/            # مكتبة المكونات الأساسية (Button, Card, Input, ...)
    │   ├── components/               # StatusPill, LanguageToggle, ThemeToggle
    │   └── utils/                    # formatters, guards, cn, jwt
    │
    ├── routes/ProtectedRoute.tsx     # حارس الصفحات حسب الدور
    ├── locales/{en,ar}/translation.json  # كل نصوص التطبيق (شرح كامل في §4.5)
    ├── styles/globals.css            # كل الـ design tokens (الألوان والخطوط) + الوضع النهاري
    └── test/                         # إعداد الاختبارات + بيانات وهمية للاختبار فقط (مش بتتحمل في التطبيق نفسه)
```

---

## 4. شرح كل جزء بالتفصيل

### 4.1 `package.json` — المكتبات المستخدمة

كل المكتبات مطابقة تمامًا لجدول التقنيات في `frontend-architecture.md` §2 (React 19, TypeScript, Vite, Tailwind v4, React Router v6, TanStack Query, Zustand, `@microsoft/signalr`, React Hook Form + Zod, react-i18next, Recharts, Axios)، بالإضافة لحاجات بسيطة ضرورية مش مذكورة صراحة في الوثيقة لكن لازمة لتنفيذها:

| مكتبة إضافية | ليه محتاجينها |
|---|---|
| `class-variance-authority` + `clsx` + `tailwind-merge` | الوثيقة نفسها بتستخدم `cva` في مثال الـ Badge (§8) — دول المكتبات اللي بتخليه شغال |
| `lucide-react` | أيقونات — مفيش مكتبة أيقونات محددة في الوثيقة، اخترت دي لأنها خفيفة وشائعة |
| `@hookform/resolvers` | الجسر بين React Hook Form و Zod (مذكور ضمنيًا في §11) |
| `i18next-browser-languagedetector` | لحفظ اختيار اللغة في localStorage (مش الـ access token — ده آمن) |

**تحذير:** نسخ الإصدارات في `package.json` (زي `react: ^19.0.0`) هي أحدث إصدارات مستقرة معروفة وقت الكتابة. لازم تشغل `npm install` وتتأكد إن مفيش تعارضات.

### 4.2 `app/providers/AuthProvider.tsx` — أهم ملف في التطبيق كله

الملف ده بيعمل 3 حاجات:

1. **Bootstrap الجلسة عند فتح الموقع:** بيحاول يعمل `POST /auth/refresh` (باستخدام الـ httpOnly cookie اللي المفروض الباك إند حطها). لو نجح، بيفك تشفير الـ JWT (`shared/utils/jwt.ts`) عشان يجيب الـ role والـ id، وبعدين يجيب البروفايل الكامل (`GET /patients/me` أو `GET /doctors/me`) عشان يعرف الاسم والإيميل. **ده معناه: أول حاجة الباك إند لازم يضمنها هي إن الـ JWT فعلاً فيه claims اسمها `sub` و`role`** بالظبط زي ما موضح في `backend-database-architecture.md` §8.
2. **ربط `apiClient`:** بيستدعي `configureApiClientAuth(...)` مرة واحدة بس عند تحميل الملف، عشان يوصل الـ axios instance (اللي في `shared/lib`) بالـ authStore (اللي في `features/auth`) من غير ما `shared/` يستورد من `features/` (قاعدة معمارية مهمة موضحة في كومنت الملف نفسه).
3. **حفظ تفضيل اللغة على الباك إند:** كل ما اللغة تتغير، بيبعت `PUT /patients/me` أو `PUT /doctors/me` مع `{ preferredLanguage }` — ده تنفيذ حرفي لـ`frontend-architecture.md` §9 ("Language preference is stored two places... persisted to the user's profile").

### 4.3 نمط "Component واحد، Chrome مختلف حسب الدور"

ده أهم قرار معماري في المشروع كله، وده إزاي اتنفذ فعليًا (مش بس كلام):

- **`features/patient-dashboard/components/PatientDetailContent.tsx`** — ده الـ component المشترك الفعلي. بياخد كل البيانات (profile, vitals, alerts) + `showNumericScore: boolean` + `headerActions: ReactNode` + مجموعة callbacks اختيارية خاصة بالطبيب (`onToggleFreezeWaveform`, `onLogManualEvent`, `onReviewStrip`). لو الـ callbacks دي مش موجودة، أزرارها مش بتظهر خالص في `EcgWaveform`.
- **`features/patient-dashboard/pages/PatientDashboardPage.tsx`** (شاشة المريض) بيرندر `PatientDetailContent` بـ `showNumericScore={false}` وزرار "اتصال بالدكتور" بس.
- **`features/doctor-dashboard/components/PatientDetailChrome.tsx`** (الطبيب) بيرندر نفس `PatientDetailContent` لكن بـ `showNumericScore={true}` + كل الـ callbacks بتاعة Freeze/Log Manual Event/Review Strip + أزرار Call/Send Message/Refresh.

ملحوظة: ده أدق من وصف `frontend-architecture.md` §5 نفسه (اللي بيقول إن الصفحتين بيرندروا نفس المكونات بشكل منفصل) — أنا فضّلت أعمل component تجميعي واحد فعلي بدل تكرار الـ layout مرتين، عشان ده بالظبط اللي `PROJECT_OVERVIEW.md` §4 و`HANDOFF_DOC-1.md` §2 بيأكدوا عليه كقرار أساسي ("مش نسختين منفصلتين من نفس الشاشة"). النتيجة: أي تعديل في ترتيب الكروت أو إضافة كارت جديد، بتعمله مرة واحدة بس في `PatientDetailContent.tsx`.

### 4.4 الموجة الحية (EcgWaveform) — تفصيل تقني

ده أعقد جزء في المشروع، وطلبته صراحة (شبكة 5×5 مم + حروف P/Q/R/S/T). اتقسم لملفين:

- **`ecgWaveformMath.ts`** — دوال رياضية خالصة (pure functions)، من غير أي canvas أو DOM، عشان تقدر تتعمللها unit tests بسهولة (وفعلاً عملتلها 12 test في `ecgWaveformMath.test.ts`). بتحتوي على:
  - `generateIdleWindow` / `idleEcgValueAt`: موجة PQRST تجريبية (شكلها رياضي معروف بالظبط) بتتعرض لما مفيش بيانات حية لسه — وده بيحصل بس قبل ما أي جهاز يبعت بيانات حقيقية، ومكتوب عليها لافتة واضحة "Awaiting live signal" فوق الموجة عشان محدش يفتكرها بيانات حقيقية.
  - `detectBeatLandmarks`: خوارزمية بسيطة بتدور على أعلى قمة (R) في آخر جزء من الموجة، وبعدين بتحسب أماكن P/Q/S/T بناءً على نسب زمنية معيارية معروفة سريريًا. **مهم:** ده تقريب بصري للعرض بس، مش قياس سريري حقيقي — الأرقام الحقيقية (PR Interval, QRS Duration, QT/QTc) بييجوا من الباك إند مباشرة (`EcgFeatureSet`) وبتتعرض كنص منفصل تحت الموجة، مش محسوبة في الفرونت إند.
- **`EcgWaveform.tsx`** — الـ component نفسه. بيرسم على `<canvas>` جوه `requestAnimationFrame` loop، بعيد تمامًا عن دورة الـ render بتاعة React (زي ما `frontend-architecture.md` §7 بيشرح بالظبط) — يعني حتى لو الجهاز بيبعت 250 عينة في الثانية، React مش بيعمل re-render 250 مرة، وده اللي بيخلي الواجهة سلسة.
  - **الشبكة (Grid):** خطوط رفيعة كل 12px (تمثل المربع الصغير 1مم)، وخط أغمق كل 5 خطوط (المربع الكبير 5×5مم) — بالظبط زي ورق تخطيط القلب الحقيقي، لكن الألوان بتاعة الثيم (تركواز على كحلي) بدل الأحمر التقليدي عشان تناسب تصميم التطبيق.
  - **القياس (Calibration):** تحت الموجة نص بـ "25 mm/s | Gain 10 mm/mV | Sample Rate 250 Hz" — أرقام ثابتة مأخوذة من مواصفات الهاردوير (AD8232/ESP32) زي ما موضح في المستندات — **لازم تتأكد من فريق الهاردوير إن الأرقام دي مطابقة للفريم وير الفعلي**.
  - **تحجيم تلقائي:** الموجة بتتحجم تلقائيًا حسب أعلى وأقل قيمة موجودة في النافذة الحالية، مش على نطاق ثابت — ده قرار مقصود عشان الرسم يفضل واضح مهما كانت وحدة القياس اللي هيبعتها الباك إند (mV, ADC counts, أو قيم متطبَّعة).

### 4.5 نظام اللغة (i18n)

- `locales/en/translation.json` و `locales/ar/translation.json` — نفس البنية بالظبط في الاتنين (اتفحصت آليًا، صفر اختلاف — تفاصيل في §7).
- النصوص العربية في صفحة اللوجن منقولة حرفيًا من `cardiosense_auth.html` الأصلي، مش مترجمة من الصفر.
- الأرقام (BPM, %, ms) بتفضل بالإنجليزية (Latin numerals) حتى في الوضع العربي — منفذة في `shared/utils/formatters.ts` عن طريق `-u-nu-latn` في الـ locale، بالظبط زي المطلوب في `frontend-architecture.md` §9.
- تغيير اللغة بيغيّر `dir`/`lang` في الـ `<html>` تلقائيًا (RTL/LTR حقيقي، مش بس نص) عن طريق `I18nProvider.tsx`.

### 4.6 الوضع الليلي/النهاري (Night Mode) — إضافة مش موجودة في الوثيقة الأصلية

طلبتها إنت صراحة في الشات ده. اتنفذت بـ:
- `shared/store/themeStore.ts` — Zustand store بسيط، متخزن في localStorage (آمن، مش زي الـ access token).
- `styles/globals.css` — قيم ألوان بديلة تحت `:root[data-theme="light"]` بنفس أسماء الـ tokens بتاعة الوضع الليلي، عشان أي مكون مكتوب أصلاً (زي `bg-surface`, `text-accent`) يشتغل صح في الوضعين من غير أي تعديل.
- زرار `ThemeToggle.tsx` موجود في: صفحة اللوجن، شريط الطبيب العلوي، شريط المريض العلوي، وصفحة الإعدادات.

### 4.7 صفحة الإعدادات

بتعرض: تفضيل اللغة، الوضع الليلي/النهاري، ربط الجهاز (للمريض بس — الطبيب مالوش جهاز)، وبيانات الحساب الأساسية. ربط الجهاز بيستخدم `POST /devices/pair` بالظبط زي الجدول في `backend-database-architecture.md` §5.

---

## 5. `shared/types/api.ts` — عقد البيانات مع الباك إند

الملف ده هو المرجع الوحيد لشكل كل DTO في التطبيق، وكل استدعاء API في المشروع بيستورد من هنا. لو الباك إند غيّر شكل أي response، هنا أول مكان تتظبط فيه الأنواع.

النقطة الأهم: `PatientProfile` مش بس بيانات ديموغرافية — هي **الشكل الكامل** اللي المفروض `GET /patients/me` و `GET /patients/{id}` يرجعوه: بروفايل + آخر قراءة حيوية + آخر تقييم AI + آخر تحقق من الأقطاب + السجل الطبي + الـ 24 ساعة الأخيرة، كله في استدعاء واحد. ده استنتجته من إن `backend-database-architecture.md` §4 بيذكر query واحدة بس اسمها `GetPatientProfile` (مفيش `GetLatestVitals` أو `GetElectrodeStatus` منفصلين)، وإن §5 بيقول صراحة إن endpoint الطبيب بيرجّع "نفس الشكل" اللي `/me` بترجعه. **لو الباك إند هيقسم البيانات دي على أكتر من endpoint، لازم نتناقش تاني.**

---

## 6. الفجوات المطلوب تأكيدها من الباك إند (أهم قسم في الملف ده)

كل نقطة هنا موجودة كمان كـ كومنت `ASSUMPTION:` أو `GAP:` جنب الكود نفسه (في `shared/types/api.ts` أو `features/*/api/*.ts`)، فمينفعش تفوتك وانت بتقرا الكود. لخصتها هنا كلها في مكان واحد:

| # | النقطة | إيه اللي المفروض يتعمل |
|---|---|---|
| 1 | **`AlertSeverity` enum** | الوثيقة بتذكر الـ enum موجود لكن من غير قيمه. افترضت `Info \| Transient \| Moderate \| Critical` بناءً على الشارات اللي ظهرت في الصور. لازم تأكيد من `Domain/Enums/AlertSeverity.cs`. |
| 2 | **الرقم الخام لدرجة الخطورة (`riskScore`)** | مش موجود كعمود صريح في جدول `RiskAssessments` بالمستند، رغم إنه ظاهر في الصور ("18/100"). افترضته موجود كـ optional field. |
| 3 | **`Log Manual Event` — بيتسجل فين؟** | مفيش جدول مخصص لـ "Manual Events" في تصميم الداتا بيز. افترضت إنه بيتسجل كـ صف جديد في جدول `Alerts` (بما إن عنده عمود `Type` عام). محتاج تأكيد. |
| 4 | **`Freeze Waveform` — إيه اللي بيحصل بالظبط؟** | بما إن الموجة الخام مش متخزنة في SQL (حسب §6)، الأمر ده على الأرجح بيتحكم في flag مؤقت في الجلسة/Redis بس، مش عملية DB حقيقية. الفرونت بيبعت `POST /patients/{id}/freeze-waveform` ومستني تأكيد فقط، مش بيعتمد على أي حاجة في الـ response. |
| 5 | **`GET /api/doctors/me`** | مش موجود في جدول الـ endpoints، بس لازم موجود عشان نعرض اسم/تخصص الطبيب في الشريط العلوي. أضفته بالقياس على `GET /api/patients/me`. |
| 6 | **`PUT /api/doctors/me`** | نفس الموضوع — محتاجينه عشان الطبيب يقدر يغيّر لغته (زي المريض بالظبط في §9). |
| 7 | **`POST /api/auth/logout`** | مش موجود في الجدول. من غيره، تسجيل الخروج هيمسح بس الـ session المحلي، والـ refresh token cookie هيفضل صالح لحد ما ينتهي لوحده. لازم endpoint يلغي الـ token من جدول `RefreshTokens` فعليًا. |
| 8 | **تقرير الذكاء الاصطناعي السردي (AI Clinical Report)** | الكارت ده ("Patient Status Narrative Summary" في الصور) مش ليه أي endpoint في الوثيقة خالص. الفرونت بيستنى `GET /patients/{id\|me}/ai-report?period=daily\|weekly\|monthly` وده endpoint جديد كليًا لازم يتضاف. |
| 9 | **"Review Strip (Last 15 min)"** | زرار موجود في التصميم، لكن بما إن الموجة الخام مش متخزنة نهائيًا (حتى في Redis غير للجلسة الحالية)، الزرار ده حاليًا بيفتح modal بيشرح إن الميزة دي مش متاحة تقنيًا لحد ما يتقرر مكان تخزين مناسب. |
| 10 | **`heartRateRange` / min-max اليومي** | الصور بتعرض "Min: 62 • Max: 98" تحت معدل النبض — ده مش موجود كحقل في `VitalsSample`. مسحت الـ UI بتاعته مؤقتًا لحد ما نقرر لو هيتحسب في الباك إند ولا الفرونت. |

---

## 7. التحقق والاختبار — الخمس لفات (5 Loops)

بما إن الـ sandbox بتاعي مفيهوش إنترنت، مقدرتش أعمل `npm install` (اتأكد ده فعليًا: أي محاولة اتصال بـ registry.npmjs.org رجعت `403 Forbidden` من الـ proxy). ده معناه مقدرتش أشغل `tsc`/`eslint`/`vitest` بشكلهم الطبيعي بالاعتماد على الـ dependencies الحقيقية. بدل ما أتجاهل التحقق، عملت الآتي:

### اللفة 1 — TypeScript Compiler حقيقي (مش وهمي)
لقيت نسخة `typescript` متاحة عالميًا في الـ sandbox (منفصلة عن dependencies المشروع). عملت ملف stub مؤقت بيدي أنواع مبسطة (لكن حقيقية) للمكتبات الخارجية (React, Zustand, Axios, ...) عشان أقدر أشغل `tsc --noEmit` فعليًا على الكود بتاعنا. **لقيت واتصلحت 3 أخطاء حقيقية:**
1. استخدام `React.RefObject` من غير ما يتم استيراد `React` كـ namespace في `useLiveEcgStream.ts`.
2. دالة `toConnectionState` متعرّفة في `SignalRProvider.tsx` بس مش مستخدمة خالص (dead code) — اتشالت مع الـ import بتاعها.
3. مشكلة حقيقية في تتبع الأنواع (type narrowing) في `apiClient.ts` بخصوص متغير `refreshInFlight` اللي بيتغير جوه closure — اتصلحت بإعادة هيكلة الكود.

الملفات المؤقتة (`__typecheck_stubs__.d.ts` و `tsconfig.verify.json`) **اتمسحت بالكامل** قبل ما أعمل الملف المضغوط — مش هتلاقيها في التسليم.

### اللفة 2 — فحص الاستيراد والتصدير (Import/Export Resolution)
كتبت سكريبت Node.js بيفحص كل الـ imports اللي بتبدأ بـ `@/` أو نسبية (`./`) في المشروع كله، ويتأكد إن كل ملف فعلاً موجود، وإن كل اسم مستورد (`{ x, y }`) فعلاً متصدّر من الملف التاني. **النتيجة: 364 استيراد اتفحص، صفر ملفات ناقصة، صفر أسماء غلط.**

### اللفة 3 — تطابق نصوص اللغة (i18n Parity + Usage)
سكريبت Python بيقارن كل الـ keys بين `en/translation.json` و`ar/translation.json` (تطابق تام مطلوب)، وبعدين بيدور على كل استدعاء `t('...')` في الكود ويتأكد إنه موجود في الملفين. **لقى مشكلتين حقيقيتين:**
- زرار الإغلاق في `Modal.tsx` كان نص إنجليزي مكتوب يدويًا ("Close") بدل ما يستخدم الترجمة — **اتصلح**.
- 18 مفتاح ترجمة متعرّف بس مش مستخدم فعليًا في أي مكان (زي شاشة "نجاح" بعد التسجيل قررت أستغنى عنها لصالح تحويل فوري للوحة التحكم) — **اتشالوا من الملفين**.
النتيجة النهائية: تطابق تام 100%، صفر مفاتيح ناقصة، صفر مفاتيح ميتة.

### اللفة 4 — فحص الـ Dependencies
قارنت كل مكتبة معرّفة في `package.json` مع الاستخدام الفعلي في الكود. **لقيت حاجة مهمة:** `recharts` كانت معرّفة كـ dependency (زي ما الوثيقة الأصلية بتطلب "Recharts... for aggregate/historical charts") بس مش مستخدمة في أي مكان — وده كشف كمان إن دالة `getPatientVitalsHistory` (اللي كانت متعمولة في `patientApi.ts` من البداية) مالهاش أي مستهلك. **الحل:** بنيت component جديد كامل `VitalsTrendChart.tsx` بيستخدم Recharts فعليًا لعرض اتجاه معدل النبض والأكسجين على مدار 24 ساعة/7 أيام/30 يوم، وربطته في `PatientDetailContent.tsx`.

### اللفة 5 — فحص التكرار والملفات اليتيمة
سكريبت بيدور على أي دالة أو مكون متعرّف مرتين في نفس الملف (نفس نوع الغلطة اللي حصلت أثناء التعديل على `authApi.ts` في نص الشات، واتصلحت وقتها). **النتيجة: صفر تكرار.**

**خلاصة صادقة:** الفحوصات دي قوية لكن مش بديل كامل عن `npm run build && npm run test` حقيقي. فيه حاجات زي الـ prop types الدقيقة لمكتبات خارجية (زي `RiskStatusPayload` اللي بييجي من SignalR) مقدرش أتأكد منها 100% من غير الـ types الحقيقية بتاعة المكتبات. **أول حاجة تعملها: `npm install && npm run build && npm run test`.**

---

## 8. قرارات وإضافات خارج الوثيقة الأصلية (وليه)

| الإضافة | السبب |
|---|---|
| `shared/utils/cn.ts` | مطلوبة لأي مكون بيستخدم `cva` (زي مثال الـ Badge في §8 من الوثيقة نفسها) |
| `shared/components/ui/{Textarea,Checkbox,PasswordInput}.tsx` | فورم التسجيل محتاجهم فعليًا (تشخيص، أدوية، كلمة سر بزرار إظهار/إخفاء) |
| `shared/store/themeStore.ts` + `ThemeToggle.tsx` | الوضع الليلي/النهاري المطلوب صراحة في الشات ده |
| `shared/hooks/useSignalR.tsx` | الـ context بتاع SignalR اتحط في `shared/` مش `app/` عشان الـ features تقدر تستهلكه من غير ما تخالف قاعدة "features بتستورد من shared بس" |
| `features/settings/api/settingsApi.ts` | فولدر settings في الوثيقة الأصلية مكنش فيه `api/` — أضفته للتناسق مع باقي الـ features |
| `PatientDetailContent.tsx` | التنفيذ الفعلي لمبدأ "component واحد" — تفصيل في §4.3 |
| `VitalsTrendChart.tsx` | اكتشفته لفة الاختبار رقم 4 — Recharts كانت معرّفة بس مش مستخدمة |

---

## 9. الخطوات الجاية المقترحة

1. **أهم حاجة:** رد على النقط الـ 8 في القسم 6 (الفجوات)، خصوصًا #3 و#4 و#8 (Manual Event, Freeze Waveform, AI Report) لأنهم بيأثروا على تصميم endpoints جديدة.
2. `npm install && npm run build && npm run test` — تأكيد إن كل حاجة شغالة في بيئة حقيقية فيها إنترنت.
3. لما الباك إند يبقى جاهز، غيّر `VITE_API_BASE_URL` و`VITE_SIGNALR_HUB_URL` في `.env` وجرب end-to-end.
4. فكر في `Playwright` لاختبارات end-to-end زي ما `frontend-architecture.md` §12 بيقترح، لما الـ flows الأساسية تستقر.

---

## 10. الملفات المسلّمة

- **`CardioSense-Frontend.zip`** — المشروع كامل (108 ملف) زي ما موضح في الشجرة بالقسم 3.
- **`HANDOFF_REPORT.md`** (الملف ده) — موجود جوه الـ zip كمان في جذر المشروع.
