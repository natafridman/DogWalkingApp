using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalkerAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalkerAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalkerId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalkerAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalkerAvailabilities_Users_WalkerId",
                        column: x => x.WalkerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalkerWorkingAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalkerId = table.Column<int>(type: "int", nullable: false),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalkerWorkingAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalkerWorkingAreas_Users_WalkerId",
                        column: x => x.WalkerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalkerAvailabilities_WalkerId",
                table: "WalkerAvailabilities",
                column: "WalkerId");

            migrationBuilder.CreateIndex(
                name: "IX_WalkerWorkingAreas_WalkerId_ZoneName",
                table: "WalkerWorkingAreas",
                columns: new[] { "WalkerId", "ZoneName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalkerAvailabilities");

            migrationBuilder.DropTable(
                name: "WalkerWorkingAreas");
        }
    }
}
