import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * Combines conditional class names (clsx) and then resolves conflicting
 * Tailwind utility classes so the *last* one wins (tailwind-merge) — e.g.
 * `cn('px-2', condition && 'px-4')` correctly yields just `px-4` when
 * `condition` is true, instead of leaving both classes present.
 *
 * This is not part of frontend-architecture.md's file list, but every `cva`-based
 * component in shared/components/ui needs it, so it lives alongside the other
 * shared utilities rather than being duplicated per component.
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
