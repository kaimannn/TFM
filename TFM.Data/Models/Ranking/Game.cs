using System;
using TFM.Data.DB;
using TFM.Data.Models.Metacritic;

namespace TFM.Data.Models.Ranking
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string CompanyName { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public int Position { get; set; }
        public int? LastPosition { get; set; }
        public int Score { get; set; }
        public Platform Platform { get; set; }
        public string ThumbnailUrl { get; set; }
        public byte[] ThumbnailBytes { get; set; }

        public Game(Games game)
        {
            Id = game.Id;
            Name = game.Name;
            ReleaseDate = game.ReleaseDate;
            CreatedOn = game.CreatedOn;
            ModifiedOn = game.ModifiedOn;
            CompanyName = game.CompanyName;
            ShortDescription = game.ShortDescription;
            LongDescription = game.LongDescription;
            Position = game.Position;
            LastPosition = game.LastPosition;
            Score = game.Score;
            Platform = (Platform)game.Platform;
            ThumbnailUrl = game.ThumbnailUrl;
            ThumbnailBytes = game.Thumbnail;
        }

        public Game(MetacriticGame game)
        {
            CompanyName = game.Developer;
            LongDescription = game.Description;
            Name = game.Title;
            Position = game.Position;
            Score = game.Score;
            ReleaseDate = Convert.ToDateTime(game.ReleaseDate);
            CreatedOn = DateTime.UtcNow;
            Platform = game.Platform;
            ThumbnailUrl = game.Image;
            ThumbnailBytes = game.ImageBytes;
        }
    }
}
