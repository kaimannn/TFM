﻿using TFM.Data.Models.Enums;

namespace TFM.Data.Models.Metacritic
{
    public class MetacriticGame
    {
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string Description { get; set; }
        public string[] Genre { get; set; }
        public string Thumbnail { get; set; }
        public int Score { get; set; }
        public string Developer { get; set; }
        public string[] Publisher { get; set; }
        public string Rating { get; set; }
        public string NumberOfPlayers { get; set; }
        public object[] AlsoAvailableOn { get; set; }
        public Platform? Platform { get; set; }
        public int Position { get; set; }
    }

}
