using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.Models
{
    public class OutfitRatingWithImage
    {
        public int Id { get; set; }
        public byte[]? OriginalImage { get; set; }
        public string Recommendation { get; set; }
        public int Score { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
