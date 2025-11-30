using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.Models
{
    public class ColorBlindType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public Filter Filter { get; set; }

        public ICollection<TestResult> TestResults { get; set; } = new HashSet<TestResult>();

    }
}
