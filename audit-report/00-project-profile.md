# نطاق مراجعة CardioSense

- الفرونت: front-end/CardioSense-Frontend-fixed (React 19 وTypeScript وVite وTailwind).
- الباك: back-end/back، ASP.NET Core 10 وEF Core وSQL Server وSignalR وMQTT وFastAPI.
- أدوار المستخدم: زائر، مريض، دكتور. JWT للطلبات وrefresh cookie للجلسة. صلاحية الملف تتحقق في الباك.
- الواجهة: /auth للزائر، /patient للمريض، /doctor/patients و/doctor/patients/:patientId للدكتور، /settings للاتنين.
- القنوات: REST للملفات والتاريخ والتنبيهات، SignalR للقراءات والحالة، MQTT للجهاز. خدمة FastAPI الداخلية للتقييم والتقرير. الدفع غير موجود.
- الاختبارات الأصلية: Vitest، xUnit لثلاثة مشاريع، وPython unittest.
- جذر المشروع خارج Git، ونسخة front Deploy مستبعدة بطلب المستخدم.
