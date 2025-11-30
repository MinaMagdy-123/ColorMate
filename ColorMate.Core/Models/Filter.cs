using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.Models
{
    public class Filter
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public int ColorBlindTypeId { get; set; }
        public ColorBlindType ColorBlindType { get; set; }

        public ICollection<ImageByUser> ImagesByUser { get; set; } = new HashSet<ImageByUser>();

    }
}
