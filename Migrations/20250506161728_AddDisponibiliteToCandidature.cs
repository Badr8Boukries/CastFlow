using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddDisponibiliteToCandidature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DispoDebut",
                table: "Candidatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DispoFin",
                table: "Candidatures",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispoDebut",
                table: "Candidatures");

            migrationBuilder.DropColumn(
                name: "DispoFin",
                table: "Candidatures");
        }
    }
}
