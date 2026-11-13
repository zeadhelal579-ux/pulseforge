import { QueryClient } from '@tanstack/react-query';

/**
 * Single QueryClient instance for the whole app (wrapped by QueryProvider).
 *
 * Defaults chosen for a clinical dashboard where most REST data (profile,
 * history, alert lists) changes slowly and the *fast-changing* numbers are
 * handled by SignalR instead of polling (frontend-architecture.md §3) — so
 * TanStack Query doesn't need an aggressive refetch interval here.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000, // 30s — REST data is "fresh enough" briefly before refetching
      retry: 1,
      refetchOnWindowFocus: false,
    },
    mutations: {
      retry: 0,
    },
  },
});
