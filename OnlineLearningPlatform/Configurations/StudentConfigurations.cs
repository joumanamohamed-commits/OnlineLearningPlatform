using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations
{
    internal class StudentConfigurations : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(S => S.Id);

            builder.Property(S => S.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(S => S.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(S => S.DateOfBirth)
                .IsRequired()
                .HasColumnType("date");
        }
    }
}
