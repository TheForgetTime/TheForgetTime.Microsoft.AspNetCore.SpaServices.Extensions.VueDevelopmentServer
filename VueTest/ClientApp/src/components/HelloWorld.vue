<template>
  <div>
    <h2>{{msg}}</h2>
    <table>
      <thead>
      <tr>
        <th>体感</th>
        <th>摄氏度</th>
        <th>华氏度</th>
        <th>时间</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="(item, i) in weatherData">
        <td>{{item.summary}}</td>
        <td>{{item.temperatureC}}</td>
        <td>{{item.temperatureF}}</td>
        <td>{{item.date}}</td>
      </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
  export default {
    name: "HelloWorld",
    data() {
      return {
        weatherData: [],
        msg: "Welecom to VueApp"
      }
    },
    created(){
      this.getData();
    },
    methods:{
      getData(){
        this.https.fetchGet("weatherforecast")
        .then((data)=>{
          this.weatherData = data.data;
        })
        .catch((err)=>{
          console.log(err)
        })
      }
    }
  }
</script>

<style scoped>

</style>
