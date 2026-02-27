using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalkDeclines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalkDeclines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalkEventId = table.Column<int>(type: "int", nullable: false),
                    WalkerId = table.Column<int>(type: "int", nullable: false),
                    DeclinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalkDeclines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalkDeclines_Users_WalkerId",
                        column: x => x.WalkerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalkDeclines_WalkEvents_WalkEventId",
                        column: x => x.WalkEventId,
                        principalTable: "WalkEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalkDeclines_WalkerId",
                table: "WalkDeclines",
                column: "WalkerId");

            migrationBuilder.CreateIndex(
                name: "IX_WalkDeclines_WalkEventId_WalkerId",
                table: "WalkDeclines",
                columns: new[] { "WalkEventId", "WalkerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalkDeclines");
        }
    }
}
