import { describe, expect, it } from 'vitest';
import { formatBpm, formatMs, formatPercent, formatSignalStrength } from '@/shared/utils/formatters';

describe('formatters', () => {
  describe('formatBpm', () => {
    it('formats English with Latin digits', () => {
      expect(formatBpm(74, 'en')).toBe('74 BPM');
    });

    it('formats Arabic but still uses Latin digits, per frontend-architecture.md §9', () => {
      const result = formatBpm(74, 'ar');
      expect(result).toContain('74'); // not "٧٤"
      expect(result).not.toMatch(/[٠-٩]/); // no Arabic-Indic digits anywhere
    });
  });

  describe('formatPercent', () => {
    it('appends % with no decimal by default', () => {
      expect(formatPercent(98, 'en')).toBe('98%');
    });

    it('respects fractionDigits', () => {
      expect(formatPercent(99.42, 'en', 1)).toBe('99.4%');
    });
  });

  describe('formatMs', () => {
    it('formats a millisecond reading in English', () => {
      expect(formatMs(152, 'en')).toBe('152 ms');
    });
  });

  describe('formatSignalStrength', () => {
    it('appends dBm to a negative signal value', () => {
      expect(formatSignalStrength(-62)).toBe('-62 dBm');
    });
  });
});
