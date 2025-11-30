using ColorMate.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.EF.Configuration
{
    public class TestQuestionsByUserConfig : IEntityTypeConfiguration<TestQuestionsByUser>
    {
        public void Configure(EntityTypeBuilder<TestQuestionsByUser> builder)
        {
            builder.HasKey(x => new { x.ApplicationUserId, x.TestQuestionId });
            builder.HasOne(x => x.ApplicationUser).WithMany(y => y.TestQuestionsByUsers).HasForeignKey(x => x.ApplicationUserId);
            builder.HasOne(x => x.TestQuestions).WithMany(y => y.TestQuestionsByUsers).HasForeignKey(x => x.TestQuestionId);

            builder.Property(x => x.SelectedAnswer).IsRequired().HasMaxLength(10);

        }
    }
}
