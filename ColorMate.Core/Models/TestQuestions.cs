using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.Models
{
    public class TestQuestions
    {
        public int Id { get; set; }
        public byte[]? Image { get; set; }
        public string? CorrectAnswer { get; set; }

        public ICollection<TestQuestionsByUser> TestQuestionsByUsers { get; set; } = new HashSet<TestQuestionsByUser>();

        
    }
}
