using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Configurations;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform
{
    internal class APPDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;DataBase=OnlineLearningPlatformDb;Trusted_Connection=true;TrustServerCertificate=true");
        }

        #region DbSets
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Instructor>(new InstructorConfigurations());
            modelBuilder.ApplyConfiguration<Category>(new CategoryConfigurations());
            modelBuilder.ApplyConfiguration<Course>(new CourseConfigurations());
            modelBuilder.ApplyConfiguration<Student>(new StudentConfigurations());
            modelBuilder.ApplyConfiguration<Enrollment>(new EnrollmentConfigurations());
        }
    }
}
