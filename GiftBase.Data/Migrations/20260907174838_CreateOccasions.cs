using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftBase.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateOccasions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OccasionId",
                table: "Gifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccasionLabel",
                table: "Gifts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccasionYear",
                table: "Gifts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Occasions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occasions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Occasions_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gifts_OccasionId",
                table: "Gifts",
                column: "OccasionId");

            migrationBuilder.CreateIndex(
                name: "IX_Occasions_PersonId_Type",
                table: "Occasions",
                columns: new[] { "PersonId", "Type" },
                unique: true,
                filter: "[Type] <> 'Custom'");

            migrationBuilder.AddForeignKey(
                name: "FK_Gifts_Occasions_OccasionId",
                table: "Gifts",
                column: "OccasionId",
                principalTable: "Occasions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gifts_Occasions_OccasionId",
                table: "Gifts");

            migrationBuilder.DropTable(
                name: "Occasions");

            migrationBuilder.DropIndex(
                name: "IX_Gifts_OccasionId",
                table: "Gifts");

            migrationBuilder.DropColumn(
                name: "OccasionId",
                table: "Gifts");

            migrationBuilder.DropColumn(
                name: "OccasionLabel",
                table: "Gifts");

            migrationBuilder.DropColumn(
                name: "OccasionYear",
                table: "Gifts");
        }
    }
}
