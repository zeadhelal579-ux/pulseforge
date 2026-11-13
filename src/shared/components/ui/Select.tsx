import { forwardRef, useId, type SelectHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/shared/utils/cn';

export interface SelectOption {
  value: string;
  label: string;
}

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: ReactNode;
  error?: string;
  required?: boolean;
  options: SelectOption[];
  /** Placeholder shown as a disabled first option, e.g. "Select gender". */
  placeholder?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ className, label, error, required, options, placeholder, id, ...props }, ref) => {
    const generatedId = useId();
    const selectId = id ?? generatedId;

    return (
      <div className="mb-4">
        {label && (
          <label htmlFor={selectId} className="mb-[7px] block text-[13px] text-text-secondary">
            {label}
            {required && <span className="ms-0.5 text-danger">*</span>}
          </label>
        )}
        <select
          ref={ref}
          id={selectId}
          aria-invalid={Boolean(error)}
          className={cn(
            'w-full rounded-[10px] border border-border bg-surface-alt px-3.5 py-[11px] text-sm text-text-primary',
            'transition-[border-color,box-shadow] focus:border-accent focus:shadow-[0_0_0_3px_var(--color-accent-subtle)] focus:outline-none',
            error && 'border-danger',
            className,
          )}
          {...props}
        >
          {placeholder && (
            <option value="" disabled>
              {placeholder}
            </option>
          )}
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        {error && <p className="mt-1.5 text-xs text-danger">{error}</p>}
      </div>
    );
  },
);
Select.displayName = 'Select';
