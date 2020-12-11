using System;
using System.Collections.Generic;

namespace TFM.Data.DB
{
    public partial class Games
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PlatformName { get; set; }
        public int Position { get; set; }
        public bool Deleted { get; set; }
        public int Score { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public int? LastPosition { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Thumbnail { get; set; }
    }
}
