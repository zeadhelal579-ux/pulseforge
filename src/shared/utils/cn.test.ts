import { describe, expect, it } from 'vitest';
import { cn } from '@/shared/utils/cn';

describe('cn', () => {
  it('joins plain class name strings', () => {
    expect(cn('px-2', 'text-sm')).toBe('px-2 text-sm');
  });

  it('drops falsy values', () => {
    expect(cn('px-2', false && 'hidden', undefined, null, 'text-sm')).toBe('px-2 text-sm');
  });

  it('lets the later conflicting Tailwind utility win', () => {
    expect(cn('px-2', 'px-4')).toBe('px-4');
  });
});
