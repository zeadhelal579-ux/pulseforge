import { forwardRef, useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Input, type InputProps } from '@/shared/components/ui/Input';

export const PasswordInput = forwardRef<HTMLInputElement, Omit<InputProps, 'type'>>(
  (props, ref) => {
    const { t } = useTranslation();
    const [visible, setVisible] = useState(false);

    return (
      <div className="relative">
        <Input ref={ref} type={visible ? 'text' : 'password'} {...props} />
        <button
          type="button"
          onClick={() => setVisible((prev) => !prev)}
          // Positioned to clear the label row above the input regardless of
          // whether an error/help line is present below it.
          className="absolute end-3.5 top-[38px] text-text-muted hover:text-text-secondary"
          aria-label={visible ? t('auth.hide') : t('auth.show')}
        >
          {visible ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
        </button>
      </div>
    );
  },
);
PasswordInput.displayName = 'PasswordInput';
