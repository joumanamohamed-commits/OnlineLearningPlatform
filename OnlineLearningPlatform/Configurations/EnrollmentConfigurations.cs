using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations
{
    internal class EnrollmentConfigurations : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            // Composite Key => a student can enroll in the same course only once
            builder.HasKey(E => new { E.StudentId, E.CourseId });

            builder.Property(E => E.EnrollmentDate)
                .IsRequired();

            builder.Property(E => E.Progress)
                .IsRequired();

            builder.Property(E => E.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(E => E.FinalGrade)
                .HasPrecision(5, 2)
                .IsRequired(false);

            // Progress must be between 0 and 100 (enforced by the database too)
            builder.ToTable(T => T.HasCheckConstraint("CK_Enrollment_Progress", "[Progress] BETWEEN 0 AND 100"));

            #region Many-to-Many Relationship (Student * ---- * Course)
            builder.HasOne(E => E.Student)
                .WithMany(S => S.Enrollments)
                .HasForeignKey(E => E.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(E => E.Course)
                .WithMany(C => C.Enrollments)
                .HasForeignKey(E => E.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a course deletes its enrollments
            #endregion
        }
    }
}
