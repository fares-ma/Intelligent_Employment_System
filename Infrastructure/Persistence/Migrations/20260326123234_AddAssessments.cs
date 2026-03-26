using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "JobPostSkills",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "RequiredLevel",
                table: "JobPostSkills",
                type: "int",
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
                name: "RequiredLevel",
                table: "JobPostSkills");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CompanyInviteCodes");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "JobPostSkills",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}
