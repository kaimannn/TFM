import Vue from "vue";
import App from "./App.vue";
import "bootstrap";
import "bootstrap/dist/css/bootstrap.min.css";
import vuetify from "./plugins/vuetify";
 
Vue.config.productionTip = false;
Vue.config.performance = process.env.NODE_ENV !== "production";

new Vue({
    vuetify,
    render: h => h(App)
}).$mount("#app");