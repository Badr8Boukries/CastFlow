using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoInstructionsToRoleAndVideoUrlToCandidature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructionsVideo",
                table: "Roles",
                type: "nvarchar(MAX)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlVideoAudition",
                table: "Candidatures",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructionsVideo",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UrlVideoAudition",
                table: "Candidatures");
        }
    }
}
