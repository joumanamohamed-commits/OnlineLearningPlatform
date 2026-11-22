namespace OnlineLearningPlatform.Models
{
    // Join entity (Student <-> Course) with its own extra data
    internal class Enrollment
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; }
        public int Progress { get; set; } // 0 - 100
        public EnrollmentStatus Status { get; set; }
        public decimal? FinalGrade { get; set; } // only when Completed
    }
}
