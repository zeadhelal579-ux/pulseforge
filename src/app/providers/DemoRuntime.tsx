/**
 * File: src/app/providers/DemoRuntime.tsx
 * Layer: app/providers (composition root)
 * Purpose: Small demo-mode-only side effects that need React context (the QueryClient and i18n).
 * Logic: A real backend returns one language for server-generated text (alert messages, AI summaries, status
 *        labels). The demo backend localises that text per request, but TanStack Query would keep showing the
 *        cached text after the visitor switches language, so this component refetches exactly those queries
 *        when the language changes. Outside demo mode it does nothing.
 * Implementation: a render-less component with one effect subscribed to i18next's `languageChanged` event.
 */
import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { IS_DEMO_MODE } from '@/shared/lib/env';

/** Query-key prefixes whose payload contains server-generated, language-dependent text. */
const LANGUAGE_DEPENDENT_QUERY_KEYS = ['patient-alerts', 'ai-report', 'patients-list'] as const;

export function DemoRuntime() {
  const queryClient = useQueryClient();
  const { i18n } = useTranslation();

  useEffect(() => {
    if (!IS_DEMO_MODE) return undefined;

    /** Refetches every cached query whose text depends on the active language. */
    const refetchLocalisedQueries = () => {
      for (const key of LANGUAGE_DEPENDENT_QUERY_KEYS) {
        void queryClient.invalidateQueries({ queryKey: [key] });
      }
    };

    i18n.on('languageChanged', refetchLocalisedQueries);
    return () => {
      i18n.off('languageChanged', refetchLocalisedQueries);
    };
  }, [i18n, queryClient]);

  return null;
}
