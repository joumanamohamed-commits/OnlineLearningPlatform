using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations
{
    internal class CourseConfigurations : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(C => C.Id);

            builder.Property(C => C.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(C => C.Description)
                .HasMaxLength(1000); // optional

            builder.Property(C => C.Price)
                .HasPrecision(10, 2);

            // Store the enum as text (Beginner / Intermediate / Advanced)
            builder.Property(C => C.Level)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(C => C.IsPublished)
                .IsRequired();

            builder.Property(C => C.PublishedDate)
                .IsRequired(false);

            #region Relationships
            // Instructor 1 ---- * Course
            builder.HasOne(C => C.Instructor)
                .WithMany(I => I.Courses)
                .HasForeignKey(C => C.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category 1 ---- * Course
            builder.HasOne(C => C.Category)
                .WithMany(Cat => Cat.Courses)
                .HasForeignKey(C => C.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            SeedData(builder);
        }

        #region Data Seeding
        // 6 courses: published + unpublished, spread across instructors, categories and levels
        private static void SeedData(EntityTypeBuilder<Course> builder)
        {
            builder.HasData(
                new Course { Id = 1, Title = "ASP.NET Core Web API", Description = "Build RESTful APIs with ASP.NET Core.", Price = 1500m, Level = CourseLevel.Advanced, IsPublished = true, PublishedDate = new DateTime(2024, 2, 1), InstructorId = 1, CategoryId = 7 },
                new Course { Id = 2, Title = "Entity Framework Core", Description = "Data access with EF Core, Code-First.", Price = 1200m, Level = CourseLevel.Intermediate, IsPublished = true, PublishedDate = new DateTime(2024, 4, 15), InstructorId = 1, CategoryId = 3 },
                new Course { Id = 3, Title = "C# Fundamentals", Description = "Start programming with C#.", Price = 600m, Level = CourseLevel.Beginner, IsPublished = true, PublishedDate = new DateTime(2023, 9, 10), InstructorId = 2, CategoryId = 1 },
                new Course { Id = 4, Title = "React From Zero", Description = "Components, hooks and state.", Price = 900m, Level = CourseLevel.Beginner, IsPublished = true, PublishedDate = new DateTime(2024, 6, 20), InstructorId = 3, CategoryId = 8 },
                new Course { Id = 5, Title = "Advanced React Patterns", Description = "Render props, compound components and more.", Price = 1300m, Level = CourseLevel.Advanced, IsPublished = false, PublishedDate = null, InstructorId = 3, CategoryId = 8 },
                new Course { Id = 6, Title = "UI/UX Design Principles", Description = null, Price = 800m, Level = CourseLevel.Intermediate, IsPublished = false, PublishedDate = null, InstructorId = 2, CategoryId = 5 }
            );
        }
        #endregion
    }
}
