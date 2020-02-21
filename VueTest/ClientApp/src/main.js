import Vue from "vue";
import App from "./App"
import router from "./router"
import https from "./utils/https";

Vue.config.productiontip = false;
Vue.prototype.https = https;

new Vue({
    el: "#app",
    router,
    components: {App},
    template: "<App/>"
});
