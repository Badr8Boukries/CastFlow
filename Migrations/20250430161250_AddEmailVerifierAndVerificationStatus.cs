using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerifierAndVerificationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "UserTalents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailVerifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerifiers_UserTalents_UserId",
                        column: x => x.UserId,
                        principalTable: "UserTalents",
                        principalColumn: "TalentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifiers_Email_VerificationCode_IsVerified_ExpiresAt",
                table: "EmailVerifiers",
                columns: new[] { "Email", "VerificationCode", "IsVerified", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifiers_UserId",
                table: "EmailVerifiers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVerifiers");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "UserTalents");
        }
    }
}
