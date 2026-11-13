/**
 * ESLint configuration.
 *
 * Two rules matter most for this codebase's correctness requirements:
 *  - "@typescript-eslint/no-unused-vars": catches declared-but-unused variables,
 *    imports, and function params (the TS compiler already enforces this via
 *    tsconfig's noUnusedLocals/noUnusedParameters — this rule gives the same
 *    feedback live in the editor and in `npm run lint`).
 *  - "react-hooks/exhaustive-deps": catches missing/incorrect dependency arrays,
 *    which is exactly the mistake frontend-architecture.md §14 warns about for
 *    the ref-based live-data hooks (useLiveEcgStream, usePatientVitals) — it's
 *    easy to accidentally add a ref to a dependency array and defeat the whole
 *    point of the ring-buffer pattern.
 */
module.exports = {
  root: true,
  env: { browser: true, es2021: true, node: true },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:react-hooks/recommended',
  ],
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 'latest',
    sourceType: 'module',
    ecmaFeatures: { jsx: true },
  },
  plugins: ['@typescript-eslint', 'react-hooks', 'react-refresh'],
  ignorePatterns: ['dist', 'node_modules', '*.config.*'],
  rules: {
    'react-refresh/only-export-components': [
      'warn',
      { allowConstantExport: true },
    ],
    '@typescript-eslint/no-unused-vars': [
      'error',
      { argsIgnorePattern: '^_', varsIgnorePattern: '^_' },
    ],
    '@typescript-eslint/no-explicit-any': 'warn',
    'react-hooks/rules-of-hooks': 'error',
    'react-hooks/exhaustive-deps': 'warn',
  },
};
