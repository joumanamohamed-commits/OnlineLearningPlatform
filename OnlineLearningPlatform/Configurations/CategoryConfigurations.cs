using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations
{
    internal class CategoryConfigurations : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(C => C.Id);

            builder.Property(C => C.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(C => C.Description)
                .HasMaxLength(300); // optional

            #region Self Relationship
            // A category has (optionally) one parent, a parent has many sub-categories.
            // Restrict => SQL Server refuses to delete a parent that still has sub-categories.
            builder.HasOne(C => C.ParentCategory)
                .WithMany(C => C.SubCategories)
                .HasForeignKey(C => C.ParentCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            SeedData(builder);
        }

        #region Data Seeding
        // 3 levels:  Programming -> Backend -> ASP.NET
        private static void SeedData(EntityTypeBuilder<Category> builder)
        {
            builder.HasData(
                // Level 1 (top-level)
                new Category { Id = 1, Name = "Programming", Description = "Everything about writing software", ParentCategoryId = null },
                new Category { Id = 2, Name = "Design", Description = "Visual and product design", ParentCategoryId = null },
                // Level 2
                new Category { Id = 3, Name = "Backend", Description = "Server-side development", ParentCategoryId = 1 },
                new Category { Id = 4, Name = "Frontend", Description = "Client-side development", ParentCategoryId = 1 },
                new Category { Id = 5, Name = "UI/UX", Description = "User interface and experience", ParentCategoryId = 2 },
                new Category { Id = 6, Name = "Mobile Development", Description = null, ParentCategoryId = 1 },
                // Level 3
                new Category { Id = 7, Name = "ASP.NET", Description = "Web development with ASP.NET Core", ParentCategoryId = 3 },
                new Category { Id = 8, Name = "React", Description = "Building UIs with React", ParentCategoryId = 4 }
            );
        }
        #endregion
    }
}
