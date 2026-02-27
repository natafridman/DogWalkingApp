using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForeignKeyBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dogs_Clients_ClientId",
                table: "Dogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WalkEvents_Dogs_DogId",
                table: "WalkEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_Dogs_Clients_ClientId",
                table: "Dogs",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WalkEvents_Dogs_DogId",
                table: "WalkEvents",
                column: "DogId",
                principalTable: "Dogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dogs_Clients_ClientId",
                table: "Dogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WalkEvents_Dogs_DogId",
                table: "WalkEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_Dogs_Clients_ClientId",
                table: "Dogs",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalkEvents_Dogs_DogId",
                table: "WalkEvents",
                column: "DogId",
                principalTable: "Dogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
