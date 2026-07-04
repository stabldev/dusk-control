// @ts-check
import { defineConfig } from "astro/config";
import tailwindcss from "@tailwindcss/vite";
import icon from "astro-icon";

const isGithub = process.env.GITHUB_ACTIONS === "true";

// https://astro.build/config
export default defineConfig({
  site: isGithub ? "https://stabldev.github.io" : undefined,
  base: isGithub ? "/dusk-control" : undefined,
  integrations: [icon()],
  vite: {
    plugins: [tailwindcss()],
  },
  devToolbar: {
    enabled: false,
  },
});
