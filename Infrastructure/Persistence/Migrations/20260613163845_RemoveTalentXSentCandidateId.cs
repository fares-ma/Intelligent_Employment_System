using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTalentXSentCandidateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_TalentXSentCandidateId_JobPostId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "TalentXSentCandidateId",
                table: "JobApplications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TalentXSentCandidateId",
                table: "JobApplications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_TalentXSentCandidateId_JobPostId",
                table: "JobApplications",
                columns: new[] { "TalentXSentCandidateId", "JobPostId" },
                filter: "[TalentXSentCandidateId] IS NOT NULL");
        }
    }
}
