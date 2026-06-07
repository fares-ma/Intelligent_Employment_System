using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTalentXResumeScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AiScoringCompletedAt",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiScoringErrorMessage",
                table: "JobApplications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiScoringRequestedAt",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiScoringStatus",
                table: "JobApplications",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "NotStarted");

            migrationBuilder.AddColumn<string>(
                name: "FitStatus",
                table: "JobApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastProcessedIdempotencyKey",
                table: "JobApplications",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchingDetailsJson",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawResumeDataJson",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TalentXSentCandidateId",
                table: "JobApplications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcessedIdempotencyKeys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    JobApplicationId = table.Column<int>(type: "int", nullable: true),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    Source = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "TalentXWebhook")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedIdempotencyKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_TalentXSentCandidateId_JobPostId",
                table: "JobApplications",
                columns: new[] { "TalentXSentCandidateId", "JobPostId" },
                filter: "[TalentXSentCandidateId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedIdempotencyKeys_IdempotencyKey",
                table: "ProcessedIdempotencyKeys",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedIdempotencyKeys");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_TalentXSentCandidateId_JobPostId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "AiScoringCompletedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "AiScoringErrorMessage",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "AiScoringRequestedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "AiScoringStatus",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "FitStatus",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "LastProcessedIdempotencyKey",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MatchingDetailsJson",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "RawResumeDataJson",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "TalentXSentCandidateId",
                table: "JobApplications");
        }
    }
}
