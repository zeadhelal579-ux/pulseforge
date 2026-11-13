export default {
  plugins: {
    // Tailwind v4 ships its own PostCSS plugin; no autoprefixer/tailwind.config.ts
    // needed — theme tokens live in src/styles/globals.css under @theme instead.
    '@tailwindcss/postcss': {},
  },
};
