using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ColorMate.Core.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<TestQuestionsByUser> TestQuestionsByUsers { get; set; } = new HashSet<TestQuestionsByUser>();
        public ICollection<ImageByUser> ImagesByUser { get; set; } = new HashSet<ImageByUser>();

        public ICollection<TestResult> TestResults { get; set; } = new HashSet<TestResult>();


    }
}
