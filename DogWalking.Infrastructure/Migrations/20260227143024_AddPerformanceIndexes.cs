using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WalkEvents_DogId_WalkDate",
                table: "WalkEvents");

            migrationBuilder.CreateIndex(
                name: "IX_WalkEvents_DogId_WalkDate_Status",
                table: "WalkEvents",
                columns: new[] { "DogId", "WalkDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WalkEvents_Status_WalkDate",
                table: "WalkEvents",
                columns: new[] { "Status", "WalkDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WalkEvents_DogId_WalkDate_Status",
                table: "WalkEvents");

            migrationBuilder.DropIndex(
                name: "IX_WalkEvents_Status_WalkDate",
                table: "WalkEvents");

            migrationBuilder.CreateIndex(
                name: "IX_WalkEvents_DogId_WalkDate",
                table: "WalkEvents",
                columns: new[] { "DogId", "WalkDate" });
        }
    }
}
