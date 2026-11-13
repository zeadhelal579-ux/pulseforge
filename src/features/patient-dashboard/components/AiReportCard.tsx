import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Download, Printer, Sparkles } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { Tabs } from '@/shared/components/ui/Tabs';
import { Button } from '@/shared/components/ui/Button';
import { getPatientAiReport } from '@/features/patient-dashboard/api/patientApi';
import { formatBpm } from '@/shared/utils/formatters';
import type { AiReportPeriod } from '@/shared/types/api';

interface AiReportCardProps {
  patientId?: string;
  patientName: string;
  className?: string;
}

/** Renders a minimal, self-contained canvas snapshot of the report and
 *  triggers a PNG download — no DOM-screenshot library is in the agreed
 *  dependency list, so this covers the "Export as Image" action without
 *  adding one. */
function exportReportAsPng(
  patientName: string,
  narrativeSummary: string,
  stats: { label: string; value: string }[],
) {
  const canvas = document.createElement('canvas');
  canvas.width = 800;
  canvas.height = 420;
  const ctx = canvas.getContext('2d');
  if (!ctx) return;

  ctx.fillStyle = '#0a0e14';
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.fillStyle = '#2dd4bf';
  ctx.font = 'bold 22px sans-serif';
  ctx.fillText('CardioSense — AI-Assisted Insights (Prototype)', 32, 50);

  ctx.fillStyle = '#8b98a9';
  ctx.font = '14px sans-serif';
  ctx.fillText(patientName, 32, 78);

  ctx.fillStyle = '#f1f5f9';
  ctx.font = '15px sans-serif';
  wrapText(ctx, narrativeSummary, 32, 120, canvas.width - 64, 22);

  stats.forEach((stat, index) => {
    const x = 32 + index * 250;
    ctx.fillStyle = '#57687a';
    ctx.font = '12px sans-serif';
    ctx.fillText(stat.label, x, 300);
    ctx.fillStyle = '#2dd4bf';
    ctx.font = 'bold 24px monospace';
    ctx.fillText(stat.value, x, 330);
  });

  canvas.toBlob((blob) => {
    if (!blob) return;
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `cardiosense-ai-report-${Date.now()}.png`;
    link.click();
    URL.revokeObjectURL(url);
  });
}

function wrapText(
  ctx: CanvasRenderingContext2D,
  text: string,
  x: number,
  y: number,
  maxWidth: number,
  lineHeight: number,
) {
  const words = text.split(' ');
  let line = '';
  let currentY = y;

  for (const word of words) {
    const testLine = line ? `${line} ${word}` : word;
    if (ctx.measureText(testLine).width > maxWidth && line) {
      ctx.fillText(line, x, currentY);
      line = word;
      currentY += lineHeight;
    } else {
      line = testLine;
    }
  }
  if (line) ctx.fillText(line, x, currentY);
}

export function AiReportCard({ patientId, patientName, className }: AiReportCardProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';
  const [period, setPeriod] = useState<AiReportPeriod>('daily');

  const { data: report, isLoading } = useQuery({
    queryKey: ['ai-report', patientId ?? 'me', period],
    queryFn: () => getPatientAiReport(period, patientId),
  });

  return (
    <Card className={className}>
      <CardHeader className="flex-wrap">
        <CardTitle className="flex items-center gap-1.5 text-sm normal-case text-text-primary">
          <Sparkles className="size-4 text-accent" />
          {t('patient.aiReport.title')}
        </CardTitle>
        <Tabs<AiReportPeriod>
          value={period}
          onChange={setPeriod}
          className="border-none"
          tabs={[
            { value: 'daily', label: t('patient.aiReport.daily') },
            { value: 'weekly', label: t('patient.aiReport.weekly') },
            { value: 'monthly', label: t('patient.aiReport.monthly') },
          ]}
        />
      </CardHeader>

      <p className="mb-4 text-xs text-text-muted">{t('patient.aiReport.subtitle')}</p>

      {isLoading ? (
        <p className="text-sm text-text-muted">{t('common.loading')}</p>
      ) : !report ? (
        <p className="text-sm text-text-muted">{t('patient.aiReport.empty')}</p>
      ) : (
        <>
          <div className="mb-4 rounded-lg bg-surface-alt p-3">
            <p className="mb-1 text-xs font-semibold text-text-muted">
              {t('patient.aiReport.narrativeSummary')}
            </p>
            <p className="text-sm leading-relaxed text-text-primary">{report.narrativeSummary}</p>
          </div>

          <div className="mb-4 grid grid-cols-3 gap-3">
            <div>
              <p className="text-xs text-text-muted">{t('patient.aiReport.averageDailyHeartRate')}</p>
              <p className="font-mono text-lg font-bold text-text-primary">
                {formatBpm(report.averageHeartRate, locale)}
              </p>
            </div>
            {patientId && report.highestRiskReading != null && <div>
              <p className="text-xs text-text-muted">{t('patient.aiReport.highestRiskReading')}</p>
              <p className="font-mono text-lg font-bold text-accent">
                {report.highestRiskReading}/100
              </p>
            </div>}
            <div>
              <p className="text-xs text-text-muted">{t('patient.aiReport.alertsLogged')}</p>
              <p className="font-mono text-lg font-bold text-warning">{report.alertsLoggedCount}</p>
            </div>
          </div>

          <div className="flex flex-wrap items-center justify-between gap-2">
            <p className="text-[11px] text-text-muted">{t('patient.aiReport.footerDisclaimer')}</p>
            <div className="flex gap-2">
              <Button
                variant="secondary"
                size="sm"
                onClick={() =>
                  exportReportAsPng(patientName, report.narrativeSummary, [
                    { label: t('patient.aiReport.averageDailyHeartRate'), value: formatBpm(report.averageHeartRate, locale) },
                    ...(patientId && report.highestRiskReading != null
                      ? [{ label: t('patient.aiReport.highestRiskReading'), value: `${report.highestRiskReading}/100` }]
                      : []),
                    { label: t('patient.aiReport.alertsLogged'), value: String(report.alertsLoggedCount) },
                  ])
                }
              >
                <Download className="size-3.5" />
                {t('patient.aiReport.exportPng')}
              </Button>
              <Button variant="secondary" size="sm" onClick={() => window.print()}>
                <Printer className="size-3.5" />
                {t('patient.aiReport.print')}
              </Button>
            </div>
          </div>
        </>
      )}
    </Card>
  );
}
