using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public DateTime TestTime { get; set; }
        public int Score { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int ColorBlindTypeId { get; set; }
        public ColorBlindType ColorBlindType { get; set; }
    }
}
