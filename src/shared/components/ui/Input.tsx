import { forwardRef, useId, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/shared/utils/cn';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: ReactNode;
  /** Shown in danger color under the field; also switches the border to --color-danger. */
  error?: string;
  /** Muted helper text shown when there's no error — e.g. "Found on the back of your device." */
  helpText?: string;
  required?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className, label, error, helpText, required, id, ...props }, ref) => {
    const generatedId = useId();
    const inputId = id ?? generatedId;

    return (
      <div className="mb-4">
        {label && (
          <label htmlFor={inputId} className="mb-[7px] block text-[13px] text-text-secondary">
            {label}
            {required && <span className="ms-0.5 text-danger">*</span>}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          aria-invalid={Boolean(error)}
          aria-describedby={error ? `${inputId}-error` : helpText ? `${inputId}-help` : undefined}
          className={cn(
            'w-full rounded-[10px] border border-border bg-surface-alt px-3.5 py-[11px] text-sm text-text-primary',
            'transition-[border-color,box-shadow] placeholder:text-text-muted',
            'focus:border-accent focus:shadow-[0_0_0_3px_var(--color-accent-subtle)] focus:outline-none',
            error && 'border-danger',
            className,
          )}
          {...props}
        />
        {error ? (
          <p id={`${inputId}-error`} className="mt-1.5 text-xs text-danger">
            {error}
          </p>
        ) : helpText ? (
          <p id={`${inputId}-help`} className="mt-1.5 text-xs text-text-muted">
            {helpText}
          </p>
        ) : null}
      </div>
    );
  },
);
Input.displayName = 'Input';
