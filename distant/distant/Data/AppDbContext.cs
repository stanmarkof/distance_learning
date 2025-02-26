﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using distant.Models;
using System.Reflection.Emit;

namespace distant.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
         public DbSet<Group> Groups { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppSetting>().HasData(
                new AppSetting
                {
                    Id = 1,
                    VerificationCode = "VLSU12" 
                });

          
            builder.Entity<User>()
                .HasDiscriminator<string>("Role")
                .HasValue<User>("User")
                .HasValue<Student>("Student")
                .HasValue<Lecturer>("Lecturer");


            
            builder.Entity<Student>()
                .HasOne(s => s.Group)
                .WithMany() 
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<Lesson>()
                .HasOne(l => l.Lecturer)
                .WithMany(l => l.Lessons) 
                .HasForeignKey(l => l.LecturerId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.Entity<Lesson>()
                .HasMany(l => l.Materials)
                .WithOne(m => m.Lesson)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Lesson>()
                .HasMany(l => l.Tests)
                .WithOne(t => t.Lesson)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Lesson>()
                .HasMany(l => l.Groups)
                .WithMany(g => g.Lessons) 
                .UsingEntity(j => j.ToTable("LessonGroups")); 


            builder.Entity<TestResult>()
                .HasOne(tr => tr.Student)
                .WithMany(s => s.TestResults)
                .HasForeignKey(tr => tr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);




        }
    }
}
