import { forwardRef, type HTMLAttributes } from 'react';
import { cn } from '@/shared/utils/cn';

/**
 * Plain panel container — no business logic, no knowledge of "patients" or
 * "risk scores" (per frontend-architecture.md §4's rule for shared/components/ui).
 * Feature components compose their own headers/content inside it.
 */
export const Card = forwardRef<HTMLDivElement, HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn('rounded-xl border border-border bg-surface p-4', className)}
      {...props}
    />
  ),
);
Card.displayName = 'Card';

export const CardHeader = forwardRef<HTMLDivElement, HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn('mb-3 flex items-center justify-between gap-2', className)}
      {...props}
    />
  ),
);
CardHeader.displayName = 'CardHeader';

export const CardTitle = forwardRef<HTMLParagraphElement, HTMLAttributes<HTMLParagraphElement>>(
  ({ className, ...props }, ref) => (
    <p
      ref={ref}
      className={cn('text-xs font-semibold text-text-muted', className)}
      {...props}
    />
  ),
);
CardTitle.displayName = 'CardTitle';
