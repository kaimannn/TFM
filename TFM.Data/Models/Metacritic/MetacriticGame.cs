using TFM.Data.Models.Ranking;

namespace TFM.Data.Models.Metacritic
{
    public class MetacriticGame
    {
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string Description { get; set; }
        public string[] Genre { get; set; }
        public string Image { get; set; }
        public int Score { get; set; }
        public string Developer { get; set; }
        public string[] Publisher { get; set; }
        public string Rating { get; set; }
        public object[] AlsoAvailableOn { get; set; }
        public Platform Platform { get; set; }
        public int Position { get; set; }
        public byte[] ImageBytes { get; set; }
    }

}
