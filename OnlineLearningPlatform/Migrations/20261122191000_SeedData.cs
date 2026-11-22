using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineLearningPlatform.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { 1, "Everything about writing software", "Programming", null },
                    { 2, "Visual and product design", "Design", null }
                });

            migrationBuilder.InsertData(
                table: "Instructors",
                columns: new[] { "Id", "Bio", "Email", "FullName", "JoinDate" },
                values: new object[,]
                {
                    { 1, "Senior .NET developer with 10 years of backend experience.", "ahmed.nasr@learnhub.com", "Ahmed Nasr", new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "Software engineer and UI/UX designer.", "mona.adel@learnhub.com", "Mona Adel", new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, null, "omar.khaled@learnhub.com", "Omar Khaled", new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { 3, "Server-side development", "Backend", 1 },
                    { 4, "Client-side development", "Frontend", 1 },
                    { 5, "User interface and experience", "UI/UX", 2 },
                    { 6, null, "Mobile Development", 1 }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { 7, "Web development with ASP.NET Core", "ASP.NET", 3 },
                    { 8, "Building UIs with React", "React", 4 }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CategoryId", "Description", "InstructorId", "IsPublished", "Level", "Price", "PublishedDate", "Title" },
                values: new object[,]
                {
                    { 1, 7, "Build RESTful APIs with ASP.NET Core.", 1, true, "Advanced", 1500m, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ASP.NET Core Web API" },
                    { 2, 3, "Data access with EF Core, Code-First.", 1, true, "Intermediate", 1200m, new DateTime(2024, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Entity Framework Core" },
                    { 3, 1, "Start programming with C#.", 2, true, "Beginner", 600m, new DateTime(2023, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "C# Fundamentals" },
                    { 4, 8, "Components, hooks and state.", 3, true, "Beginner", 900m, new DateTime(2024, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "React From Zero" },
                    { 5, 8, "Render props, compound components and more.", 3, false, "Advanced", 1300m, null, "Advanced React Patterns" },
                    { 6, 5, null, 2, false, "Intermediate", 800m, null, "UI/UX Design Principles" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6 });

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[] { 7, 8 });

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[] { 3, 4, 5, 6 });

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "Instructors",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3 });
        }
    }
}
