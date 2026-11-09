import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: { "/api": { target: "http://localhost:5208", changeOrigin: true } },
  },
  test: {
    testTimeout: 10000,
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
    css: false,
    restoreMocks: true,
    reporters: ["default", "junit"],
    outputFile: { junit: "output/tests/frontend.junit.xml" },
  },
});
