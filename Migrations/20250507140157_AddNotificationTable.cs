using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DestinataireTalentId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EstLu = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeEntiteLiee = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdEntiteLiee = table.Column<long>(type: "bigint", nullable: true),
                    LienNavigationFront = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_UserTalents_DestinataireTalentId",
                        column: x => x.DestinataireTalentId,
                        principalTable: "UserTalents",
                        principalColumn: "TalentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DestinataireTalentId",
                table: "Notifications",
                column: "DestinataireTalentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
