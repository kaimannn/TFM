<template>
    <v-container>
        <v-row 
            align="center"
            justify="center"
        >
            <v-col md="2" sm="2" cols="4">
                <img :src="game.thumbnail"/>
            </v-col>
            <v-col md="6" sm="6" cols="8">
                <tr class="text-left" style="font-weight:bold; font-size:16pt;">{{ game.position }}. {{ game.name }}</tr>
                <tr class="text-left"><span style="font-weight:bold; font-size:10pt;">Desarrollado por: </span><span style="font-size:10pt; color:#757575;">{{ game.companyName }}</span></tr>
                <tr class="text-left" style="font-size:10pt; color:#757575;">{{new Date(game.releaseDate).toLocaleString('es-ES', { year: 'numeric', month: 'long', day: '2-digit' })}}</tr>
            </v-col>            
            <v-col md="2" sm="2" cols="6">
                <tr>
                    <v-img :src="require('../assets/emptystar.png')"  height="64px" width="64px">
                        <td style="font-family:Impact; font-weight:bold; font-size:20pt; color:#002846; ">{{game.score}}</td>
                    </v-img>
                </tr>
            </v-col>
            <v-col md="2" sm="2" cols="6">
                <v-btn
                    elevation="2"
                    color="#febd69"
                    style="font-weight:bold;"
                    href="https://www.amazon.es/gp/product/B01M9COSMD/ref=as_li_tl?ie=UTF8&camp=3638&creative=24630&creativeASIN=B01M9COSMD&linkCode=as2&tag=losmejoresjuegos-21&linkId=8108f01abdcefc4fbfaf2c116664da3d"
                    target="_blank"
                >Amazon</v-btn>
            </v-col>
        </v-row> 
    </v-container>
</template>

<script>
    export default {
        name: "Game",   
        props: ['game'], 
    }
</script>

<style scoped>
    td {
        padding-left: 17px;
        padding-top: 15px;
    }
</style>