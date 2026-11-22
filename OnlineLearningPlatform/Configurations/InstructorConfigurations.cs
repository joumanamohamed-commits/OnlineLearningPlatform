using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations
{
    internal class InstructorConfigurations : IEntityTypeConfiguration<Instructor>
    {
        public void Configure(EntityTypeBuilder<Instructor> builder)
        {
            builder.HasKey(I => I.Id);

            builder.Property(I => I.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(I => I.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(I => I.Bio)
                .HasMaxLength(500); // optional

            builder.Property(I => I.JoinDate)
                .IsRequired()
                .HasColumnType("date");

            SeedData(builder);
        }

        #region Data Seeding
        private static void SeedData(EntityTypeBuilder<Instructor> builder)
        {
            builder.HasData(
                new Instructor { Id = 1, FullName = "Ahmed Nasr", Email = "ahmed.nasr@learnhub.com", Bio = "Senior .NET developer with 10 years of backend experience.", JoinDate = new DateTime(2023, 1, 15) },
                new Instructor { Id = 2, FullName = "Mona Adel", Email = "mona.adel@learnhub.com", Bio = "Software engineer and UI/UX designer.", JoinDate = new DateTime(2023, 6, 1) },
                new Instructor { Id = 3, FullName = "Omar Khaled", Email = "omar.khaled@learnhub.com", Bio = null, JoinDate = new DateTime(2024, 3, 10) }
            );
        }
        #endregion
    }
}
