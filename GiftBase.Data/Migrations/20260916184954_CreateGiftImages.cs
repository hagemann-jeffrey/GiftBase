using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftBase.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateGiftImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageVersion",
                table: "Gifts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GiftImages",
                columns: table => new
                {
                    GiftId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftImages", x => x.GiftId);
                    table.ForeignKey(
                        name: "FK_GiftImages_Gifts_GiftId",
                        column: x => x.GiftId,
                        principalTable: "Gifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftImages");

            migrationBuilder.DropColumn(
                name: "ImageVersion",
                table: "Gifts");
        }
    }
}
