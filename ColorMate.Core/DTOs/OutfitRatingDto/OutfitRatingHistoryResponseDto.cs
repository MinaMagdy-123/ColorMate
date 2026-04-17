using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.DTOs.OutfitRatingDto
{
    public class OutfitRatingHistoryResponseDto
    {
        public string ImageBase64 { get; set; }
        public string Recommendation { get; set; }
        public int Score { get; set; }
    }
}
