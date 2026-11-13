import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { Tabs } from '@/shared/components/ui/Tabs';
import { usePatientVitalsHistory } from '@/features/patient-dashboard/hooks/usePatientVitalsHistory';
import { formatTimestamp } from '@/shared/utils/formatters';
import type { VitalsHistoryRange } from '@/features/patient-dashboard/types';

interface VitalsTrendChartProps {
  patientId?: string;
  className?: string;
}

export function VitalsTrendChart({ patientId, className }: VitalsTrendChartProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';
  const [range, setRange] = useState<VitalsHistoryRange>('24h');

  const { data: history, isLoading } = usePatientVitalsHistory(range, patientId);

  const chartData = (history ?? []).map((sample) => ({
    timestamp: sample.timestamp,
    label: formatTimestamp(sample.timestamp, locale),
    heartRate: sample.heartRate,
    spo2: sample.spo2,
  }));

  return (
    <Card className={className}>
      <CardHeader className="flex-wrap">
        <CardTitle className="text-sm normal-case text-text-primary">
          {t('patient.vitalsTrend.title')}
        </CardTitle>
        <Tabs<VitalsHistoryRange>
          value={range}
          onChange={setRange}
          className="border-none"
          tabs={[
            { value: '24h', label: t('patient.vitalsTrend.range24h') },
            { value: '7d', label: t('patient.vitalsTrend.range7d') },
            { value: '30d', label: t('patient.vitalsTrend.range30d') },
          ]}
        />
      </CardHeader>

      {isLoading ? (
        <p className="text-sm text-text-muted">{t('common.loading')}</p>
      ) : chartData.length < 2 ? (
        <p className="text-sm text-text-muted">{t('patient.vitalsTrend.empty')}</p>
      ) : (
        <div className="h-[220px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={chartData} margin={{ top: 4, right: 8, left: -16, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
              <XAxis
                dataKey="label"
                tick={{ fontSize: 10, fill: 'var(--color-text-muted)' }}
                minTickGap={40}
              />
              <YAxis tick={{ fontSize: 10, fill: 'var(--color-text-muted)' }} width={32} />
              <Tooltip
                contentStyle={{
                  background: 'var(--color-surface)',
                  border: '1px solid var(--color-border)',
                  borderRadius: 8,
                  fontSize: 12,
                }}
              />
              <Line
                type="monotone"
                dataKey="heartRate"
                name={t('patient.vitalsTrend.heartRate')}
                stroke="var(--color-accent)"
                dot={false}
                strokeWidth={2}
              />
              <Line
                type="monotone"
                dataKey="spo2"
                name={t('patient.vitalsTrend.spo2')}
                stroke="var(--color-warning)"
                dot={false}
                strokeWidth={2}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}
    </Card>
  );
}
