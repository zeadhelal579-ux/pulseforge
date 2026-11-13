import { forwardRef, type ButtonHTMLAttributes } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { Loader2 } from 'lucide-react';
import { cn } from '@/shared/utils/cn';

export const buttonVariants = cva(
  'inline-flex items-center justify-center gap-2 rounded-[10px] font-semibold ' +
    'transition-colors disabled:pointer-events-none disabled:opacity-50 ' +
    'focus-visible:outline focus-visible:outline-2 focus-visible:outline-accent focus-visible:outline-offset-2',
  {
    variants: {
      variant: {
        // Filled teal — the auth page's .btn-primary (submit buttons, primary CTAs).
        // Text color (#04211D) is copied verbatim from cardiosense_auth.html's
        // .btn-primary rule rather than approximated from the --color-bg token.
        primary: 'bg-accent-strong text-[#04211D] hover:bg-accent active:scale-[0.99]',
        // Bordered, used for secondary actions like "Call" / "Send Message".
        secondary:
          'border border-border-strong bg-transparent text-text-primary hover:border-accent hover:text-accent',
        // No background/border at all — icon buttons, toolbar actions.
        ghost: 'bg-transparent text-text-secondary hover:bg-surface-alt hover:text-text-primary',
        // Destructive/urgent actions — "Urgent Call" on a critical patient row.
        danger: 'bg-danger/10 text-danger hover:bg-danger/20',
        // Inline text link — "Forgot password?", "Create an account".
        link: 'bg-transparent p-0 font-bold text-accent hover:underline',
      },
      size: {
        sm: 'px-3 py-1.5 text-xs',
        md: 'px-4 py-2.5 text-sm',
        lg: 'w-full px-4 py-[13px] text-[15px]',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
    },
  },
);

export interface ButtonProps
  extends ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  /** Shows a spinner and disables the button — for async submit/mutation actions. */
  isLoading?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, isLoading, disabled, children, ...props }, ref) => {
    return (
      <button
        ref={ref}
        className={cn(buttonVariants({ variant, size }), className)}
        disabled={disabled || isLoading}
        aria-busy={isLoading}
        {...props}
      >
        {isLoading && <Loader2 className="size-4 animate-spin" aria-hidden="true" />}
        {children}
      </button>
    );
  },
);
Button.displayName = 'Button';
