import { forwardRef, useId, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/shared/utils/cn';

export interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: ReactNode;
  error?: string;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ className, label, error, id, ...props }, ref) => {
    const generatedId = useId();
    const inputId = id ?? generatedId;

    return (
      <div className="mb-4">
        <label htmlFor={inputId} className="flex cursor-pointer items-start gap-2.5">
          <input
            ref={ref}
            id={inputId}
            type="checkbox"
            aria-invalid={Boolean(error)}
            className={cn(
              'mt-0.5 size-4 shrink-0 rounded border-border bg-surface-alt text-accent',
              'accent-[var(--color-accent)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-accent',
              className,
            )}
            {...props}
          />
          <span className="text-[13px] leading-snug text-text-secondary">{label}</span>
        </label>
        {error && <p className="mt-1.5 text-xs text-danger">{error}</p>}
      </div>
    );
  },
);
Checkbox.displayName = 'Checkbox';
