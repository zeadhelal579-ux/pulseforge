import { forwardRef, useId, type TextareaHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/shared/utils/cn';

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: ReactNode;
  error?: string;
  helpText?: string;
  required?: boolean;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
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
        <textarea
          ref={ref}
          id={inputId}
          aria-invalid={Boolean(error)}
          className={cn(
            'min-h-[74px] w-full resize-y rounded-[10px] border border-border bg-surface-alt px-3.5 py-[11px] text-sm text-text-primary',
            'transition-[border-color,box-shadow] placeholder:text-text-muted',
            'focus:border-accent focus:shadow-[0_0_0_3px_var(--color-accent-subtle)] focus:outline-none',
            error && 'border-danger',
            className,
          )}
          {...props}
        />
        {error ? (
          <p className="mt-1.5 text-xs text-danger">{error}</p>
        ) : helpText ? (
          <p className="mt-1.5 text-xs text-text-muted">{helpText}</p>
        ) : null}
      </div>
    );
  },
);
Textarea.displayName = 'Textarea';
