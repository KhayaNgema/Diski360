using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class AddDivisionToTournament : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Tournament",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tournament_DivisionId",
                table: "Tournament",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tournament_Divisions_DivisionId",
                table: "Tournament",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tournament_Divisions_DivisionId",
                table: "Tournament");

            migrationBuilder.DropIndex(
                name: "IX_Tournament_DivisionId",
                table: "Tournament");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Tournament");
        }
    }
}
