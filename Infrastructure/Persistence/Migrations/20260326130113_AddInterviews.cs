using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiTranscript",
                table: "Interviews");

            migrationBuilder.AlterColumn<string>(
                name: "MeetingLink",
                table: "Interviews",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "Interviews",
                type: "int",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 60);

            migrationBuilder.AddColumn<string>(
                name: "AiAnswers",
                table: "Interviews",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CompanyInviteCodes",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAnswers",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CompanyInviteCodes");

            migrationBuilder.AlterColumn<string>(
                name: "MeetingLink",
                table: "Interviews",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "Interviews",
                type: "int",
                nullable: false,
                defaultValue: 60,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 30);

            migrationBuilder.AddColumn<string>(
                name: "AiTranscript",
                table: "Interviews",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
