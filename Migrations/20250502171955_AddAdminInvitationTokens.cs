using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminInvitationTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminInvitationTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActivationToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    InvitedByAdminId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAdminId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminInvitationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminInvitationTokens_UserAdmins_CreatedAdminId",
                        column: x => x.CreatedAdminId,
                        principalTable: "UserAdmins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdminInvitationTokens_UserAdmins_InvitedByAdminId",
                        column: x => x.InvitedByAdminId,
                        principalTable: "UserAdmins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminInvitationTokens_ActivationToken",
                table: "AdminInvitationTokens",
                column: "ActivationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminInvitationTokens_CreatedAdminId",
                table: "AdminInvitationTokens",
                column: "CreatedAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminInvitationTokens_Email",
                table: "AdminInvitationTokens",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AdminInvitationTokens_InvitedByAdminId",
                table: "AdminInvitationTokens",
                column: "InvitedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminInvitationTokens");
        }
    }
}
