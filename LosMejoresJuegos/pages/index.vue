<template>
  <v-app>
    <v-app-bar 
      color="green" 
      height="80" 
      hide-on-scroll 
      app 
    >
      <v-img position="left" :src=logo max-height="80" :max-width="maxWidthTitle" contain></v-img>
    </v-app-bar>
    <v-main 
      class="yellow lighten-2"
    >
      <!-- <v-container
        :max-width="maxWidthVCard"
        style="padding-top: 50px;"
      >
        <h3 style="color:#002846;"><i>"En LosMejoresJuegos.NET encontrarás los mejores juegos de todos los tiempos para PS4, PS5, XBox Series, Nintendo Switch y PC. Los rankings se han generado en base a las mejores puntuaciones que han recibido estos videojuegos a través de diferentes medios especializados del sector.
          No dejes escapar las obras maestras que se han desarrollado para tus consolas preferidas y nunca dejes de ser un niño, nunca dejes de jugar."</i>
        </h3>
      </v-container> -->
      
      <v-card
        class="mx-auto my-12 elevation-12"
        :max-width="maxWidthVCard"
        dark
      >
        <v-tabs
          background-color="#002846"
          color="#ffe646"
          height="75px"
        >
          <v-tab
            style="font-size: 16px;"
            v-for="item in platforms"
            :key="item.id"
          >
            {{ item.name }}
          </v-tab>
          <v-tab-item
            v-for="item in platforms"
            :key="item.id"
          >
            <Ranking :platform="item.name" :numgames="100" />
          </v-tab-item>
        </v-tabs>
      </v-card>
    </v-main>
    <v-footer
      class="font-weight-medium"
    >
      <v-spacer></v-spacer>
      <div>LosMejoresJuegos.NET &copy; {{ new Date().getFullYear() }}</div>
    </v-footer> 
  </v-app>
</template>

<script>
  import Ranking from '@/components/Ranking';

  export default {
    name: 'App',
    components: {
      Ranking,
    },
    computed: {
      maxWidthVCard () {
        switch (this.$vuetify.breakpoint.name) {
          case 'lg': return '80%'
          case 'xl': return '60%'
          default: return '100%'
        }
      },
      maxWidthTitle () {
        switch (this.$vuetify.breakpoint.name) {
          case 'xs': return '90%'
          default: return '350'
        }
      },
    },
    data: () => ({
      logo: require('@/assets/logo.png'),
      platforms:[
        {
            "id":0,
            "name":"PS4"
        },
        {
            "id":2,
            "name":"Switch"
        }
      ]
    }),
    head() {
      return {
        htmlAttrs: {
          lang: "es-ES",
        },
      };
    }
  };
</script>
