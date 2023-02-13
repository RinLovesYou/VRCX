import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import AutoImport from 'unplugin-auto-import/vite';
import Components from 'unplugin-vue-components/vite';
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers';
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
import vuePugPlugin from 'vue-pug-plugin';
import pugPlugin from 'vite-plugin-pug';
import vue from '@vitejs/plugin-vue';

const options = {};
const locals = { name: 'My Pug' };

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vuePugPlugin,
    pugPlugin(options, locals),
    AutoImport({
      resolvers: [ElementPlusResolver()]
    }),
    Components({
      resolvers: [ElementPlusResolver()]
    })
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
});
