using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftBase.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateGiftSuggestionQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiftSuggestionQuotas",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RequestCount = table.Column<int>(type: "int", nullable: false),
                    WindowStartedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftSuggestionQuotas", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_GiftSuggestionQuotas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftSuggestionQuotas");
        }
    }
}
