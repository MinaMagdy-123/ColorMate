using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.Models
{
    public class ImageByUser
    {
        public int Id { get; set; }
        public byte[]? OriginalImage { get; set; }
        public OutfitRating OutfitRating { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicaationUser { get; set; }

        public byte[]? ProcessedImageByFilter { get; set; }

        public int? FilterId { get; set; }
        public Filter? Filter { get; set; }


    }
}
