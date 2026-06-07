using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPostNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DegreesJson",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "JobPosts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceMaxYears",
                table: "JobPosts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceMinYears",
                table: "JobPosts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperiencePriority",
                table: "JobPosts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GPA",
                table: "JobPosts",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GPAPriority",
                table: "JobPosts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolesJson",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkillsJson",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DegreesJson",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "ExperienceMaxYears",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "ExperienceMinYears",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "ExperiencePriority",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "GPA",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "GPAPriority",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "RolesJson",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "SkillsJson",
                table: "JobPosts");
        }
    }
}
