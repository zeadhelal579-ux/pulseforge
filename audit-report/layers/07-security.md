---
layer: 7
name: مراجعة الأمان
status: Failed
mode: executed
tests_run: 0
tests_passed: 0
tests_failed: 0
critical: 0
high: 1
medium: 0
low: 0
info: 0
---

npm audit وNuGet وpip-audit رجّعوا صفر تنبيهات بعد التحديثات. راجع L07-001 لعزل الأجهزة في النشر.

### L07-001 — عزل هوية أجهزة MQTT في النشر

- **Severity:** High
- القراءة بتدخل سجل طبي.
- المصدر: mosquitto/mosquitto.conf وdocker-compose.yml. الدليل مراجعة إعدادات.
- المتوقع: كل جهاز عنده اعتماد مستقل وACL للـ serial الخاص بيه، مع إمكانية إلغائه.
- الفعلي: Compose تطوير فيه اعتماد مشترك، وparser بيثق في الـ serial بعد التحقق من إنه مسجل ومقترن. اتقيدت منافذ SQL وMQTT على loopback، وعزل كل جهاز على broker نشر فعلي لسه مطلوب.
- الأثر: صلاحية نشر مشتركة ما بتقدمش عزل أجهزة كافي لقراءات طبية.
- المعالجة المتبقية: تجهيز اعتمادات الأجهزة وسياسات النشر على الـ broker الفعلي.
- **Effort:** M

