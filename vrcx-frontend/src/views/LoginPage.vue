<!-- eslint-disable @typescript-eslint/no-explicit-any -->
<script lang="ts">
import { defineComponent, inject } from 'vue';
import emitter from '../eventBus';
import { ElMessage as Message } from 'element-plus';

const enum AuthResult {
  Error = 'Error',
  Success = 'Success',
  TotpRequired = 'TotpRequired',
  EmailRequired = 'EmailRequired'
}

export default defineComponent({
  name: 'LoginPage',
  data() {
    return {
      username: '',
      password: '',
      rememberMe: false,
      loading: false,
      totpRequired: false,
      emailRequired: false,
      code: '',
      rules: {
        username: [
          {
            required: true,
            message: 'Please input your username',
            trigger: 'blur'
          }
        ],
        password: [
          {
            required: true,
            message: 'Please input your password',
            trigger: 'blur'
          }
        ],
        code: [
          {
            required: true,
            message: 'Please input your TOTP code',
            trigger: 'blur',
            validator: (rule: any, value: any, callback: any) => {
              if (value.length !== 6) {
                callback(new Error('TOTP code must be 6 digits'));
              } else {
                callback();
              }
            }
          }
        ]
      }
    };
  },
  setup() {
    return { ws: inject('ws') as WebSocket };
  },
  methods: {
    loginSubmit() {
      this.loading = true;
      console.log('loginSubmit');
      if ((this.totpRequired || this.emailRequired) && this.code.length !== 6) {
        Message.error('2FA code must be 6 digits');
        this.loading = false;
        return;
      }
      if (this.totpRequired) {
        this.ws.send(
          JSON.stringify({
            type: 'LoginTotp',
            rememberMe: this.rememberMe,
            code: this.code
          })
        );
      } else if (this.emailRequired) {
        this.ws.send(
          JSON.stringify({
            type: 'LoginEmail',
            rememberMe: this.rememberMe,
            code: this.code
          })
        );
      } else {
        this.ws.send(
          JSON.stringify({
            type: 'Login',
            username: this.username,
            password: this.password,
            rememberMe: this.rememberMe
          })
        );
      }
    }
  },
  beforeMount() {
    Message.error('test');
    emitter.on('Login', (data: any) => {
      const authResult = data.authResult as AuthResult;
      const errorMessage = data.message;
      if (authResult === AuthResult.Error) {
        Message.error(`Login failed: ${errorMessage}`);
        this.loading = false;
        return;
      }
      console.log('login component received login event', data);
      this.code = '';
      this.totpRequired = false;
      this.loading = false;
      if (authResult === AuthResult.Success) {
        Message.success('Login successful');
        this.$router.push('/');
      } else if (authResult === AuthResult.TotpRequired) {
        Message.info('Login TOTP required');
        this.totpRequired = true;
      } else if (authResult === AuthResult.EmailRequired) {
        Message.info('Login 2FA Email required');
        this.emailRequired = true;
      } else {
        Message.error(`Login failed ${data.message}`);
      }
    });
  },
  beforeUnmount() {
    emitter.off('login');
  }
});
</script>

<template lang="pug">
.x-login-container
  div(style="width: 300px; margin: auto;")
    div(style="margin: 15px;")
      h2 Login
      el-form(ref="loginForm" :rules="rules" @submit.native.prevent="loginSubmit()")
        el-form-item(label="Username" prop="username" required)
          el-input(v-model="username" name="username" placeholder="Username" clearable)
        el-form-item(label="Password" prop="password" required style="margin-top:10px")
          el-input(type="password" v-model="password" name="password" placeholder="Password" clearable show-password)
        el-checkbox(v-model="rememberMe" style="margin-top:15px") Remember me
        template(v-if="totpRequired")
          el-form-item(label="2FA TOTP Code" prop="TOTP Code" required style="margin-top:10px")
            el-input(v-model="code" name="totp" placeholder="TOTP Code" clearable)
        template(v-if="emailRequired")
          el-form-item(label="2FA Email Code" prop="Email Code" required style="margin-top:10px")
            el-input(v-model="code" name="email" placeholder="Email Code" clearable)
        el-form-item(style="margin-top:15px")
          el-button(native-type="submit" type="primary" :loading="loading" style="width:100%") Login

</template>

<style scoped>
.x-login-container {
  position: absolute;
  z-index: 1999;
  display: flex;
  width: 100%;
  height: 100%;
}

h2 {
  font-weight: bold;
  text-align: center;
  margin: 0px;
}
/* h1 {
  font-weight: 500;
  font-size: 2.6rem;
  top: -10px;
}

h3 {
  font-size: 1.2rem;
}

.greetings h1,
.greetings h3 {
  text-align: center;
}

@media (min-width: 1024px) {
  .greetings h1,
  .greetings h3 {
    text-align: left;
  }
} */
</style>
