#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace MyField.Migrations
{
    public partial class AddUniqueIndexesForClubRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing indexes if they exist
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClubManager_ClubId",
                table: "AspNetUsers");


            // Create a unique index for ClubManager role
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClubManager_ClubId",
                table: "AspNetUsers",
                column: "ClubId",
                unique: false);

            // Create a unique index for ClubAdministrator role
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClubAdministrator_ClubId",
                table: "AspNetUsers",
                column: "ClubId",
                unique: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the indexes when rolling back
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClubManager_ClubId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClubAdministrator_ClubId",
                table: "AspNetUsers");
        }
    }
}
