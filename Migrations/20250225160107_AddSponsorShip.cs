using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SponsorDetails",
                table: "Tournament",
                newName: "Sponsorship");

            migrationBuilder.AddColumn<string>(
                name: "SponsorContactDetails",
                table: "Tournament",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SponsorContactDetails",
                table: "Tournament");

            migrationBuilder.RenameColumn(
                name: "Sponsorship",
                table: "Tournament",
                newName: "SponsorDetails");
        }
    }
}
