using System;
using System.Text;
using TFM.Data.DB;
using TFM.Data.Models.Enums;
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
        public string Description { get; set; }
        public int Position { get; set; }
        public int? LastPosition { get; set; }
        public int Score { get; set; }
        public Platform Platform { get; set; }
        public string Thumbnail { get; set; }
        public bool Deleted { get; set; }

        public Game(Games game)
        {
            Id = game.Id;
            Name = game.Name;
            ReleaseDate = game.ReleaseDate;
            CreatedOn = game.CreatedOn;
            ModifiedOn = game.ModifiedOn;
            CompanyName = game.CompanyName;
            Description = game.Description;
            Position = game.Position;
            LastPosition = game.LastPosition;
            Score = game.Score;
            Platform = (Platform)Enum.Parse(typeof(Platform), game.PlatformName);
            Thumbnail = game.Thumbnail;
            Deleted = game.Deleted;
        }

        public Game(MetacriticGame game)
        {
            CompanyName = game.Developer;
            Description = game.Description;
            Name = game.Title;
            Position = game.Position;
            Score = game.Score;
            ReleaseDate = Convert.ToDateTime(game.ReleaseDate);
            CreatedOn = DateTime.UtcNow;
            Platform = game.Platform.Value;
            Thumbnail = game.Thumbnail;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Id: {Id}");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"CreatedOn: {CreatedOn}");
            sb.AppendLine($"ModifiedOn: {ModifiedOn}");
            sb.AppendLine($"ReleaseDate: {ReleaseDate}");
            sb.AppendLine($"CompanyName: {CompanyName}");
            sb.AppendLine($"Description: {Description}");
            sb.AppendLine($"Position: {Position}");
            sb.AppendLine($"LastPosition: {LastPosition}");
            sb.AppendLine($"Score: {Score}");
            sb.AppendLine($"Platform: {Platform}");
            return sb.ToString();
        }
    }
}
