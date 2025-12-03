using ColorMate.Core.Models;
using ColorMate.EF.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.EF
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<TestQuestions> TestQuestions { get; set; }
        public DbSet<TestQuestionsByUser> TestQuestionsByUsers { get; set; }
        public DbSet<ColorBlindType> ColorBlindTypes { get; set; }

        public DbSet<ImageByUser> ImagesByUsers { get; set; }
        public DbSet<OutfitRating> OutfitRatings { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Filter> Filters { get; set; }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestQuestionsByUserConfig).Assembly);

            modelBuilder.Entity<ColorBlindType>()
                 .HasOne(c => c.Filter)
                 .WithOne(f => f.ColorBlindType)
                 .HasForeignKey<Filter>(x => x.ColorBlindTypeId);


            modelBuilder.Entity<ImageByUser>()
                .HasOne(i => i.OutfitRating)
                .WithOne(o => o.ImageByUser)
                .HasForeignKey<OutfitRating>(x => x.ImageByUserId);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ImagesByUser)
                .WithOne(i => i.ApplicationUser)
                .HasForeignKey(i => i.ApplicationUserId);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.TestResults)
                .WithOne(t => t.ApplicationUser)
                .HasForeignKey(t => t.ApplicationUserId);


            modelBuilder.Entity<ColorBlindType>()
                .HasMany(c => c.TestResults)
                .WithOne(t => t.ColorBlindType)
                .HasForeignKey(t => t.ColorBlindTypeId);


            modelBuilder.Entity<Filter>()
                .HasMany(f => f.ImagesByUser)
                .WithOne(i => i.Filter)
                .HasForeignKey(i => i.FilterId);


            base.OnModelCreating(modelBuilder);
        }
    }
}
