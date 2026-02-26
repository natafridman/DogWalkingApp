using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WalkEventEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalkEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DogId = table.Column<int>(type: "int", nullable: false),
                    WalkerId = table.Column<int>(type: "int", nullable: true),
                    WalkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, defaultValue: "General"),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalkEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalkEvents_Dogs_DogId",
                        column: x => x.DogId,
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WalkEvents_Users_WalkerId",
                        column: x => x.WalkerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalkEvents_DogId_WalkDate",
                table: "WalkEvents",
                columns: new[] { "DogId", "WalkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WalkEvents_WalkerId",
                table: "WalkEvents",
                column: "WalkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalkEvents");
        }
    }
}
