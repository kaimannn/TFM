<template>
  <v-data-table
    :headers="headers"
    :items="games"
    :loading="loading"
    :server-items-length="totalGames"
    :hide-default-header="true"
    :hide-default-footer="true"
    :show-expand="true"
    class="elevation-1"
  >
    <template v-slot:[`item.game`]="{ item }">
      <Game :game="item"/>
    </template>
    <template v-slot:expanded-item="{ headers, item }">
        <td :colspan="headers.length">
            <br>
            <tr align="justify">{{item.description}}</tr>
            <br>
        </td>
    </template>
  </v-data-table>
</template>

<script>
  import axios from "axios";
  import Game from '@/components/Game';

  export default {
    name: "Ranking",   
    props: ['platform', 'numgames'], 
    components: {
      Game,
    },
    data() {
      return {
        totalGames: 0,
        games: [],
        loading: true,
        headers: [
          { text: "Game", value: "game" }
        ],
      };
    },
    methods: {
      getGames() {
        this.loading = true;        
        axios
          .get("https://tfmwebapi.azurewebsites.net/platforms/" + this.platform + "?NumGamesToShow=" + this.numgames)
          .then((response) => {
            this.loading = false;
            this.games = response.data;
            this.totalGames = this.numgames;
          });
      }
    },    
    mounted() {
      this.getGames();
    },
  };
</script>

<style scoped>
</style>
