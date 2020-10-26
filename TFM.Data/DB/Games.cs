﻿using System;

namespace TFM.Data.DB
{
    public partial class Games
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
        public int Platform { get; set; }
        public string ThumbnailUrl { get; set; }
        public byte[] Thumbnail { get; set; }
    }
}
