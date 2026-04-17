using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.DTOs.OutfitRatingDto
{
    public class OutfitRatingResponseDto
    {
        public string Recommendation { get; set; }
        public int Score { get; set; }
    }
}
