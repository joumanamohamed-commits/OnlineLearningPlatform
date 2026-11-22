namespace OnlineLearningPlatform.Models
{
    internal class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        // Self Relationship: a category may have a parent (null => top-level)
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new HashSet<Category>();

        // One Category -> Many Courses
        public ICollection<Course> Courses { get; set; } = new HashSet<Course>();
    }
}
