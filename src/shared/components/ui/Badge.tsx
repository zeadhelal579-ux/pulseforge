import type { HTMLAttributes } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/shared/utils/cn';

/**
 * One source of truth for every status pill in the app (patient dashboard,
 * doctor's patient list, alert timeline). Add a fourth status later (e.g.
 * "Offline") by adding one variant here — frontend-architecture.md §8/§15.
 */
export const badgeVariants = cva(
  'inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold whitespace-nowrap',
  {
    variants: {
      status: {
        // Stable reuses the brand teal rather than a separate green (§8).
        stable: 'bg-accent-subtle text-accent',
        caution: 'bg-warning/10 text-warning',
        critical: 'bg-danger/10 text-danger',
        // Generic, non-clinical variants for badges that aren't a risk status
        // (e.g. the "Prototype" research badge on the auth page hero).
        neutral: 'bg-surface-alt text-text-secondary',
        info: 'bg-accent-subtle text-accent',
      },
    },
    defaultVariants: {
      status: 'neutral',
    },
  },
);

export interface BadgeProps extends HTMLAttributes<HTMLSpanElement>, VariantProps<typeof badgeVariants> {
  /** Renders a small solid dot before the label — matches "● Stable" in the mockups. */
  withDot?: boolean;
}

export function Badge({ className, status, withDot, children, ...props }: BadgeProps) {
  return (
    <span className={cn(badgeVariants({ status }), className)} {...props}>
      {withDot && <span className="size-1.5 rounded-full bg-current" aria-hidden="true" />}
      {children}
    </span>
  );
}
