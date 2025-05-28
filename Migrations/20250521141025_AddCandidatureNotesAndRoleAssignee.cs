using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidatureNotesAndRoleAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Projets_ProjetId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_AdminInvitationTokens_ActivationToken",
                table: "AdminInvitationTokens");

            migrationBuilder.DropIndex(
                name: "IX_AdminInvitationTokens_Email",
                table: "AdminInvitationTokens");

            migrationBuilder.AddColumn<long>(
                name: "TalentAssigneId",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EstLu",
                table: "Notifications",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NoteMoyenne",
                table: "Candidatures",
                type: "decimal(2,1)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminCandidatureNotes",
                columns: table => new
                {
                    NoteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatureId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    NoteValue = table.Column<decimal>(type: "decimal(2,1)", nullable: false),
                    DateNote = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminCandidatureNotes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_AdminCandidatureNotes_Candidatures_CandidatureId",
                        column: x => x.CandidatureId,
                        principalTable: "Candidatures",
                        principalColumn: "CandidatureId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminCandidatureNotes_UserAdmins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "UserAdmins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TalentAssigneId",
                table: "Roles",
                column: "TalentAssigneId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminCandidatureNotes_AdminId",
                table: "AdminCandidatureNotes",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminCandidatureNotes_CandidatureId_AdminId",
                table: "AdminCandidatureNotes",
                columns: new[] { "CandidatureId", "AdminId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Projets_ProjetId",
                table: "Roles",
                column: "ProjetId",
                principalTable: "Projets",
                principalColumn: "ProjetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_UserTalents_TalentAssigneId",
                table: "Roles",
                column: "TalentAssigneId",
                principalTable: "UserTalents",
                principalColumn: "TalentId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Projets_ProjetId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_UserTalents_TalentAssigneId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "AdminCandidatureNotes");

            migrationBuilder.DropIndex(
                name: "IX_Roles_TalentAssigneId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "TalentAssigneId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "NoteMoyenne",
                table: "Candidatures");

            migrationBuilder.AlterColumn<bool>(
                name: "EstLu",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_AdminInvitationTokens_ActivationToken",
                table: "AdminInvitationTokens",
                column: "ActivationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminInvitationTokens_Email",
                table: "AdminInvitationTokens",
                column: "Email");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Projets_ProjetId",
                table: "Roles",
                column: "ProjetId",
                principalTable: "Projets",
                principalColumn: "ProjetId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
