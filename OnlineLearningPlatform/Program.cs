using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                #region Part 02 : Data Seeding (from code)
                PrintHeader("Data Seeding");
                await SeedStudentsAsync();
                await SeedEnrollmentsAsync();
                #endregion

                #region Part 03 : Operations

                #region Create (C)
                PrintHeader("CREATE - Add an instructor");
                int? newInstructorId = await AddInstructorAsync("Rana Hatem", "rana.hatem@learnhub.com", "EF Core and SQL Server instructor.");

                PrintHeader("CREATE - Add a course");
                int? newCourseId = await AddCourseAsync("Clean Architecture in .NET", 1400m, CourseLevel.Advanced, instructorId: newInstructorId ?? 1, categoryId: 7);
                await AddCourseAsync("Ghost Course", 500m, CourseLevel.Beginner, instructorId: 9999, categoryId: 7); // instructor does not exist
                await AddCourseAsync("Ghost Course", 500m, CourseLevel.Beginner, instructorId: 1, categoryId: 9999); // category does not exist

                PrintHeader("CREATE - Add several students (one AddRange, one SaveChanges)");
                List<int> newStudentIds = await AddStudentsAsync();

                PrintHeader("CREATE - Enroll a student");
                await EnrollStudentAsync(newStudentIds[0], 1);    // OK
                await EnrollStudentAsync(newStudentIds[0], 1);    // already enrolled
                await EnrollStudentAsync(newStudentIds[0], 5);    // course not published
                await EnrollStudentAsync(9999, 1);                // student not found
                await EnrollStudentAsync(newStudentIds[0], 9999); // course not found
                #endregion

                #region Read | Retrieve (R)
                PrintHeader("READ - Find a course");
                await FindCourseByIdAsync(2);
                await FindCourseByIdAsync(9999);
                await FindCourseByTitleAsync("Entity Framework Core");
                await FindCourseByTitleAsync("Cooking 101");

                PrintHeader("READ - Filter courses by price (700 - 1400)");
                await FilterCoursesByPriceAsync(700m, 1400m);

                PrintHeader("READ - Sub-categories");
                await PrintSubCategoriesAsync(1);
                await PrintSubCategoriesAsync(9999);
                await PrintTopLevelCategoriesAsync();

                PrintHeader("READ - Course roster");
                await PrintCourseRosterAsync(1);
                #endregion

                #region Update (U)
                int courseId = newCourseId ?? 0;

                PrintHeader("UPDATE - Change a price");
                await ChangeCoursePriceAsync(courseId, 1250m);
                await ChangeCoursePriceAsync(9999, 100m);  // not found
                await ChangeCoursePriceAsync(courseId, -5m); // invalid

                PrintHeader("UPDATE - Update progress");
                await UpdateProgressAsync(newStudentIds[0], 1, 55);
                await UpdateProgressAsync(newStudentIds[0], 1, 100, finalGrade: 88.5m); // becomes Completed
                await UpdateProgressAsync(newStudentIds[0], 1, 150); // rejected
                await UpdateProgressAsync(9999, 1, 50);              // not found

                PrintHeader("UPDATE - Publish a course");
                await PublishCourseAsync(courseId);
                await PublishCourseAsync(courseId); // already published
                #endregion

                #region Delete | Remove (D)
                // Prepare: now that the new course is published, enroll the 3 new students in it
                PrintHeader("DELETE - (setup) enroll the new students in the new course");
                foreach (int studentId in newStudentIds)
                    await EnrollStudentAsync(studentId, courseId);

                PrintHeader("DELETE - Unenroll");
                await UnenrollAsync(newStudentIds[2], courseId);
                await UnenrollAsync(9999, courseId); // not found

                PrintHeader("DELETE - Delete a category (unsafe, catches DbUpdateException)");
                await DeleteCategoryUnsafeAsync(1); // "Programming" has sub-categories

                PrintHeader("DELETE - Delete a category (safe)");
                await SafeDeleteCategoryAsync(1); // refused: has sub-categories
                await SafeDeleteCategoryAsync(6); // "Mobile Development": no sub-categories, no courses

                PrintHeader("DELETE - Delete a course that has enrollments");
                await DeleteCourseAsync(courseId);
                #endregion

                #region Bulk Operations
                PrintHeader("BULK - Raise price of published Beginner courses by 10%");
                await BulkRaisePriceAsync(CourseLevel.Beginner);

                PrintHeader("BULK - Delete all Dropped enrollments");
                await BulkDeleteDroppedEnrollmentsAsync();
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                // e.g. SQL Server is not running or the database was not created (Update-Database)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Something went wrong: {ex.Message}");
                Console.ResetColor();
            }
        }

        // NOTE: every operation opens its own short-lived APPDbContext.
        // This keeps the Change Tracker clean between operations
        // (e.g. a failed delete does not stay "Deleted" and break the next SaveChanges).

        #region Data Seeding
        private static async Task SeedStudentsAsync()
        {
            using APPDbContext dbContext = new APPDbContext();

            if (!await dbContext.Students.AnyAsync())
            {
                dbContext.Students.AddRange(
                    new Student { FullName = "Sara Mohamed", Email = "sara.mohamed@mail.com", DateOfBirth = new DateTime(2001, 3, 12) },
                    new Student { FullName = "Youssef Ali", Email = "youssef.ali@mail.com", DateOfBirth = new DateTime(2000, 7, 25) },
                    new Student { FullName = "Noha Hassan", Email = "noha.hassan@mail.com", DateOfBirth = new DateTime(2002, 11, 3) },
                    new Student { FullName = "Karim Samy", Email = "karim.samy@mail.com", DateOfBirth = new DateTime(1999, 1, 18) },
                    new Student { FullName = "Laila Fathy", Email = "laila.fathy@mail.com", DateOfBirth = new DateTime(2003, 5, 30) });
                await dbContext.SaveChangesAsync();
                Console.WriteLine("Students Data Seed (5 students)");
            }
            else
                Console.WriteLine("Students already exist -> seeding skipped");
        }

        private static async Task SeedEnrollmentsAsync()
        {
            using APPDbContext dbContext = new APPDbContext();

            if (await dbContext.Enrollments.AnyAsync())
            {
                Console.WriteLine("Enrollments already exist -> seeding skipped");
                return;
            }

            // Ids are generated by the database, so get them by email
            var ids = await dbContext.Students
                .Select(S => new { S.Id, S.Email })
                .ToDictionaryAsync(S => S.Email, S => S.Id);

            string[] emails = { "sara.mohamed@mail.com", "youssef.ali@mail.com", "noha.hassan@mail.com", "karim.samy@mail.com", "laila.fathy@mail.com" };
            if (emails.Any(E => !ids.ContainsKey(E)))
            {
                Console.WriteLine("Seed students were not found -> enrollments seeding skipped");
                return;
            }

            int sara = ids[emails[0]], youssef = ids[emails[1]], noha = ids[emails[2]], karim = ids[emails[3]], laila = ids[emails[4]];

            // Only published courses (1, 2, 3, 4) - covers Active, Completed and Dropped
            dbContext.Enrollments.AddRange(
                new Enrollment { StudentId = sara, CourseId = 1, EnrollmentDate = new DateTime(2025, 1, 10), Progress = 40, Status = EnrollmentStatus.Active },
                new Enrollment { StudentId = sara, CourseId = 2, EnrollmentDate = new DateTime(2024, 9, 1), Progress = 100, Status = EnrollmentStatus.Completed, FinalGrade = 92.5m },
                new Enrollment { StudentId = youssef, CourseId = 3, EnrollmentDate = new DateTime(2024, 10, 5), Progress = 100, Status = EnrollmentStatus.Completed, FinalGrade = 85m },
                new Enrollment { StudentId = youssef, CourseId = 4, EnrollmentDate = new DateTime(2025, 2, 14), Progress = 65, Status = EnrollmentStatus.Active },
                new Enrollment { StudentId = noha, CourseId = 1, EnrollmentDate = new DateTime(2025, 3, 1), Progress = 10, Status = EnrollmentStatus.Dropped },
                new Enrollment { StudentId = noha, CourseId = 3, EnrollmentDate = new DateTime(2025, 3, 20), Progress = 20, Status = EnrollmentStatus.Active },
                new Enrollment { StudentId = karim, CourseId = 2, EnrollmentDate = new DateTime(2025, 4, 2), Progress = 5, Status = EnrollmentStatus.Dropped },
                new Enrollment { StudentId = laila, CourseId = 4, EnrollmentDate = new DateTime(2024, 12, 12), Progress = 100, Status = EnrollmentStatus.Completed, FinalGrade = 97.75m });
            await dbContext.SaveChangesAsync();
            Console.WriteLine("Enrollments Data Seed (8 enrollments)");
        }
        #endregion

        #region Create (C)
        private static async Task<int?> AddInstructorAsync(string fullName, string email, string? bio)
        {
            using APPDbContext dbContext = new APPDbContext();

            Instructor instructor = new Instructor()
            {
                FullName = fullName,
                Email = email,
                Bio = bio,
                JoinDate = DateTime.Today
            };

            dbContext.Instructors.Add(instructor);
            Console.WriteLine($"Before SaveChanges -> Id = {instructor.Id}");
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"After SaveChanges  -> Instructor '{instructor.FullName}' added with Id = {instructor.Id}");
            return instructor.Id;
        }

        private static async Task<int?> AddCourseAsync(string title, decimal price, CourseLevel level, int instructorId, int categoryId)
        {
            using APPDbContext dbContext = new APPDbContext();

            if (!await dbContext.Instructors.AnyAsync(I => I.Id == instructorId))
            {
                Console.WriteLine($"Cannot add '{title}': instructor with Id {instructorId} does not exist.");
                return null;
            }
            if (!await dbContext.Categories.AnyAsync(C => C.Id == categoryId))
            {
                Console.WriteLine($"Cannot add '{title}': category with Id {categoryId} does not exist.");
                return null;
            }

            Course course = new Course()
            {
                Title = title,
                Price = price,
                Level = level,
                IsPublished = false,   // not published yet => PublishedDate stays null
                PublishedDate = null,
                InstructorId = instructorId, // working with FK properties directly
                CategoryId = categoryId
            };

            dbContext.Courses.Add(course);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Course '{course.Title}' added with Id = {course.Id} (Instructor {instructorId}, Category {categoryId})");
            return course.Id;
        }

        private static async Task<List<int>> AddStudentsAsync()
        {
            using APPDbContext dbContext = new APPDbContext();

            List<Student> students = new List<Student>()
            {
                new Student { FullName = "Mostafa Ibrahim", Email = "mostafa.ibrahim@mail.com", DateOfBirth = new DateTime(2001, 8, 9) },
                new Student { FullName = "Hana Tarek", Email = "hana.tarek@mail.com", DateOfBirth = new DateTime(2002, 2, 21) },
                new Student { FullName = "Ziad Mahmoud", Email = "ziad.mahmoud@mail.com", DateOfBirth = new DateTime(2000, 12, 1) }
            };

            dbContext.Students.AddRange(students); // one AddRange
            await dbContext.SaveChangesAsync();    // one SaveChanges

            foreach (Student student in students)
                Console.WriteLine($"Student '{student.FullName}' added with Id = {student.Id}");

            return students.Select(S => S.Id).ToList();
        }

        private static async Task EnrollStudentAsync(int studentId, int courseId)
        {
            using APPDbContext dbContext = new APPDbContext();

            if (!await dbContext.Students.AnyAsync(S => S.Id == studentId))
            {
                Console.WriteLine($"Enrollment refused: student with Id {studentId} does not exist.");
                return;
            }

            Course? course = await dbContext.Courses.FindAsync(courseId);
            if (course is null)
            {
                Console.WriteLine($"Enrollment refused: course with Id {courseId} does not exist.");
                return;
            }
            if (!course.IsPublished)
            {
                Console.WriteLine($"Enrollment refused: course '{course.Title}' is not published yet.");
                return;
            }
            if (await dbContext.Enrollments.AnyAsync(E => E.StudentId == studentId && E.CourseId == courseId))
            {
                Console.WriteLine($"Enrollment refused: student {studentId} is already enrolled in '{course.Title}'.");
                return;
            }

            dbContext.Enrollments.Add(new Enrollment()
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now,
                Progress = 0,
                Status = EnrollmentStatus.Active
            });
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Student {studentId} enrolled in '{course.Title}' successfully.");
        }
        #endregion

        #region Read | Retrieve (R)
        private static async Task FindCourseByIdAsync(int id)
        {
            using APPDbContext dbContext = new APPDbContext();

            Course? course = await dbContext.Courses.FindAsync(id);
            if (course is null)
                Console.WriteLine($"Course with Id {id}: not found");
            else
                Console.WriteLine($"Find({id})  -> {course.Id} :: {course.Title} :: {course.Level} :: {course.Price:N2}");
        }

        private static async Task FindCourseByTitleAsync(string title)
        {
            using APPDbContext dbContext = new APPDbContext();

            Course? course = await dbContext.Courses.AsNoTracking().FirstOrDefaultAsync(C => C.Title == title);
            if (course is null)
                Console.WriteLine($"Course with title '{title}': not found");
            else
                Console.WriteLine($"FirstOrDefault('{title}') -> {course.Id} :: {course.Title} :: {course.Level} :: {course.Price:N2}");
        }

        private static async Task FilterCoursesByPriceAsync(decimal minPrice, decimal maxPrice)
        {
            if (minPrice > maxPrice)
            {
                Console.WriteLine("Invalid range: minimum price is greater than maximum price.");
                return;
            }

            using APPDbContext dbContext = new APPDbContext();

            var courses = await dbContext.Courses
                .Where(C => C.Price >= minPrice && C.Price <= maxPrice)
                .OrderByDescending(C => C.Price)
                .Select(C => new { C.Title, C.Level, C.Price })
                .ToListAsync();

            if (courses.Count == 0)
            {
                Console.WriteLine("No courses in this price range.");
                return;
            }

            foreach (var course in courses)
                Console.WriteLine($"{course.Title,-30} | {course.Level,-12} | {course.Price,10:N2}");
        }

        private static async Task PrintSubCategoriesAsync(int categoryId)
        {
            using APPDbContext dbContext = new APPDbContext();

            Category? category = await dbContext.Categories.FindAsync(categoryId);
            if (category is null)
            {
                Console.WriteLine($"Category with Id {categoryId}: not found");
                return;
            }

            var subCategories = await dbContext.Categories
                .Where(C => C.ParentCategoryId == categoryId)
                .ToListAsync();

            Console.WriteLine($"Direct sub-categories of '{category.Name}':");
            if (subCategories.Count == 0)
                Console.WriteLine("   (none)");
            foreach (var sub in subCategories)
                Console.WriteLine($"   - {sub.Id} :: {sub.Name}");
        }

        private static async Task PrintTopLevelCategoriesAsync()
        {
            using APPDbContext dbContext = new APPDbContext();

            var topLevel = await dbContext.Categories
                .Where(C => C.ParentCategoryId == null)
                .ToListAsync();

            Console.WriteLine("Top-level categories:");
            foreach (var category in topLevel)
                Console.WriteLine($"   - {category.Id} :: {category.Name}");
        }

        private static async Task PrintCourseRosterAsync(int courseId)
        {
            using APPDbContext dbContext = new APPDbContext();

            Course? course = await dbContext.Courses.FindAsync(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId}: not found");
                return;
            }

            // 1) Student Ids from the enrollments (no joins)
            List<int> studentIds = await dbContext.Enrollments
                .Where(E => E.CourseId == courseId)
                .Select(E => E.StudentId)
                .ToListAsync();

            // 2) The students whose Id is in that list => WHERE Id IN (...)
            var students = await dbContext.Students
                .Where(S => studentIds.Contains(S.Id))
                .Select(S => S.FullName)
                .ToListAsync();

            Console.WriteLine($"'{course.Title}' has {studentIds.Count} enrolled student(s):");
            foreach (string name in students)
                Console.WriteLine($"   - {name}");
        }
        #endregion

        #region Update (U)
        private static async Task ChangeCoursePriceAsync(int courseId, decimal newPrice)
        {
            if (newPrice < 0)
            {
                Console.WriteLine($"Invalid price {newPrice}: price cannot be negative.");
                return;
            }

            using APPDbContext dbContext = new APPDbContext();

            Course? course = await dbContext.Courses.FindAsync(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId}: not found");
                return;
            }

            decimal oldPrice = course.Price;
            course.Price = newPrice;
            Console.WriteLine($"State after change = {dbContext.Entry(course).State}"); // Modified
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"'{course.Title}' price changed from {oldPrice:N2} to {course.Price:N2}");
        }

        private static async Task UpdateProgressAsync(int studentId, int courseId, int newProgress, decimal? finalGrade = null)
        {
            if (newProgress < 0 || newProgress > 100)
            {
                Console.WriteLine($"Rejected: progress {newProgress} is outside 0 - 100.");
                return;
            }

            using APPDbContext dbContext = new APPDbContext();

            // Find with the composite key (same order as HasKey: StudentId, CourseId)
            Enrollment? enrollment = await dbContext.Enrollments.FindAsync(studentId, courseId);
            if (enrollment is null)
            {
                Console.WriteLine($"Enrollment (Student {studentId}, Course {courseId}): not found");
                return;
            }

            enrollment.Progress = newProgress;
            if (newProgress == 100)
            {
                enrollment.Status = EnrollmentStatus.Completed;
                enrollment.FinalGrade = finalGrade; // the grade is given only when completed
            }
            else if (enrollment.Status == EnrollmentStatus.Completed)
            {
                enrollment.Status = EnrollmentStatus.Active;
                enrollment.FinalGrade = null;
            }

            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Enrollment (Student {studentId}, Course {courseId}) -> Progress = {enrollment.Progress}%, Status = {enrollment.Status}"
                              + (enrollment.FinalGrade is null ? "" : $", Grade = {enrollment.FinalGrade}"));
        }

        private static async Task PublishCourseAsync(int courseId)
        {
            using APPDbContext dbContext = new APPDbContext();

            Course? course = await dbContext.Courses.FindAsync(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId}: not found");
                return;
            }
            if (course.IsPublished)
            {
                Console.WriteLine($"'{course.Title}' is already published (since {course.PublishedDate:d}).");
                return;
            }

            course.IsPublished = true;
            course.PublishedDate = DateTime.Now;
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"'{course.Title}' published on {course.PublishedDate:g}");
        }
        #endregion

        #region Delete | Remove (D)
        private static async Task UnenrollAsync(int studentId, int courseId)
        {
            using APPDbContext dbContext = new APPDbContext();

            Enrollment? enrollment = await dbContext.Enrollments.FindAsync(studentId, courseId);
            if (enrollment is null)
            {
                Console.WriteLine($"Enrollment (Student {studentId}, Course {courseId}): not found");
                return;
            }

            dbContext.Enrollments.Remove(enrollment);
            Console.WriteLine($"State = {dbContext.Entry(enrollment).State}"); // Deleted
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Student {studentId} unenrolled from course {courseId}.");
        }

        private static async Task DeleteCategoryUnsafeAsync(int categoryId)
        {
            using APPDbContext dbContext = new APPDbContext();

            Category? category = await dbContext.Categories.FindAsync(categoryId);
            if (category is null)
            {
                Console.WriteLine($"Category with Id {categoryId}: not found");
                return;
            }

            try
            {
                dbContext.Categories.Remove(category);
                await dbContext.SaveChangesAsync(); // SQL Server rejects it (FK with Restrict / NO ACTION)
                Console.WriteLine($"Category '{category.Name}' deleted.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Could not delete '{category.Name}'.");
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner exception  : {ex.InnerException?.Message}");
            }
        }

        private static async Task SafeDeleteCategoryAsync(int categoryId)
        {
            using APPDbContext dbContext = new APPDbContext();

            Category? category = await dbContext.Categories.FindAsync(categoryId);
            if (category is null)
            {
                Console.WriteLine($"Category with Id {categoryId}: not found");
                return;
            }

            int subCategoriesCount = await dbContext.Categories.CountAsync(C => C.ParentCategoryId == categoryId);
            if (subCategoriesCount > 0)
            {
                Console.WriteLine($"Cannot delete '{category.Name}': it still has {subCategoriesCount} sub-categories.");
                return;
            }

            // Courses -> Category is also Restrict, so check it as well
            int coursesCount = await dbContext.Courses.CountAsync(C => C.CategoryId == categoryId);
            if (coursesCount > 0)
            {
                Console.WriteLine($"Cannot delete '{category.Name}': it still has {coursesCount} courses.");
                return;
            }

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Category '{category.Name}' deleted safely.");
        }

        private static async Task DeleteCourseAsync(int courseId)
        {
            using APPDbContext dbContext = new APPDbContext();

            Course? course = await dbContext.Courses.FindAsync(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId}: not found");
                return;
            }

            int before = await dbContext.Enrollments.CountAsync(E => E.CourseId == courseId);
            Console.WriteLine($"Enrollments of '{course.Title}' before delete = {before}");

            dbContext.Courses.Remove(course);
            await dbContext.SaveChangesAsync();

            // Why were the enrollments removed?
            // Enrollment -> Course is a REQUIRED relationship configured with DeleteBehavior.Cascade
            // (see EnrollmentConfigurations), so the FK in the database is "ON DELETE CASCADE".
            // When SQL Server deletes the course row, it automatically deletes every enrollment
            // that points to it (an enrollment cannot exist without its course).
            int after = await dbContext.Enrollments.CountAsync(E => E.CourseId == courseId);
            Console.WriteLine($"Course '{course.Title}' deleted.");
            Console.WriteLine($"Enrollments of '{course.Title}' after delete  = {after}");
        }
        #endregion

        #region Bulk Operations
        private static async Task BulkRaisePriceAsync(CourseLevel level)
        {
            using APPDbContext dbContext = new APPDbContext();

            // One UPDATE statement, no entities loaded into memory
            int affected = await dbContext.Courses
                .Where(C => C.IsPublished && C.Level == level)
                .ExecuteUpdateAsync(S => S.SetProperty(C => C.Price, C => C.Price * 1.10m));

            Console.WriteLine($"Price raised by 10% for {affected} published {level} course(s).");
        }

        private static async Task BulkDeleteDroppedEnrollmentsAsync()
        {
            using APPDbContext dbContext = new APPDbContext();

            // One DELETE statement, no entities loaded into memory
            int affected = await dbContext.Enrollments
                .Where(E => E.Status == EnrollmentStatus.Dropped)
                .ExecuteDeleteAsync();

            Console.WriteLine($"{affected} dropped enrollment(s) deleted.");
        }
        #endregion

        #region Helpers
        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"===== {title} =====");
            Console.ResetColor();
        }
        #endregion
    }
}
