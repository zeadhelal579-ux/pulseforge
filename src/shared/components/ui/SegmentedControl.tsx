import { cn } from '@/shared/utils/cn';

export interface SegmentedOption<T extends string> {
  value: T;
  label: string;
  /** Optional trailing count badge, e.g. "Stable 34" on the patient-list filter. */
  count?: number;
}

interface SegmentedControlProps<T extends string> {
  options: SegmentedOption<T>[];
  value: T;
  onChange: (value: T) => void;
  className?: string;
  'aria-label': string;
}

/**
 * Generic over T so the same component serves the 2-option role toggle and
 * the 4-option status filter without ad-hoc string typing at each call site.
 */
export function SegmentedControl<T extends string>({
  options,
  value,
  onChange,
  className,
  'aria-label': ariaLabel,
}: SegmentedControlProps<T>) {
  return (
    <div
      role="tablist"
      aria-label={ariaLabel}
      className={cn(
        'flex gap-1 rounded-xl border border-border bg-surface-alt p-1',
        className,
      )}
    >
      {options.map((option) => {
        const isActive = option.value === value;
        return (
          <button
            key={option.value}
            type="button"
            role="tab"
            aria-selected={isActive}
            onClick={() => onChange(option.value)}
            className={cn(
              'flex-1 rounded-lg px-3 py-2 text-sm font-semibold transition-colors',
              isActive
                ? 'bg-accent-subtle text-accent'
                : 'text-text-secondary hover:text-text-primary',
            )}
          >
            {option.label}
            {typeof option.count === 'number' && (
              <span className="ms-1.5 font-mono text-xs opacity-80">{option.count}</span>
            )}
          </button>
        );
      })}
    </div>
  );
}
