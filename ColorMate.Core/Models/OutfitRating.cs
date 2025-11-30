using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.Models
{
    public class OutfitRating
    {
        public int Id { get; set; }
        public string Recommendation { get; set; }
        public int Score { get; set; }

        public int ImageByUserId { get; set; }
        public ImageByUser ImageByUser { get; set; }
    }
}
