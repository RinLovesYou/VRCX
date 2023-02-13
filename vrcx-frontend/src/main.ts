import { createApp } from 'vue';
import ElementPlus from 'element-plus';
import { ElMessage } from 'element-plus';
import App from './App.vue';
import router from './router';

import 'element-plus/dist/index.css';
import 'element-plus/theme-chalk/dark/css-vars.css';
import './assets/main.css';

import webSocket from './websocket';
import emitter from './eventBus';
emitter.on('*', (type, e) => console.log(type, e));

ElMessage.install = (app: any) => {
  app.config.globalProperties.$message = ElMessage;
};

const app = createApp(App);

app.provide('ws', webSocket());
// app.config.globalProperties.$ws = webSocket();
// declare module '@vue/runtime-core' {
//   interface ComponentCustomProperties {
//     store: WebSocket;
//   }
// }

app.use(ElementPlus);
app.use(router);

app.mount('#app');
