# بيئة الاختبار

- Windows، Node 24.15، .NET SDK 10.0.402، Python 3.11، SQL Server LocalDB 17، Edge وChrome.
- الفرونت على http://localhost:5173؛ Vite يمرّر /api و/hubs إلى https://localhost:5001.
- FastAPI على 127.0.0.1:8000، وbroker الاختبار على 127.0.0.1:18883. استخدم الاختبار MQTT 3.1.1 على loopback.
- قواعد SQL مستقلة للتشغيل واختبارات التكامل والـ migration. الحسابات والقراءات اصطناعية.
- أوامر إعادة الاختبار: npm ci، npm run build، npm run lint، npm test -- --maxWorkers=2، npm audit. في الباك: dotnet build CardioSense.sln -c Release، dotnet test CardioSense.sln -c Release، python -B -m unittest discover -s tests -t .
- لتشغيل تكامل .NET بدون Docker، اضبط CARDIOSENSE_TEST_SQL على قاعدة اختبار مخصصة. سكربتات المتصفح من جذر المشروع: node tests-audit/patient-browser.mjs --mqtt وnode tests-audit/doctor-browser.mjs.
- Git status رجّع أن جذر المشروع خارج Git. فولدر front Deploy خارج نطاق العمل.
