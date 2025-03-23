using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTournamentRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "TournamentRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRules_TournamentId",
                table: "TournamentRules",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRules_Tournament_TournamentId",
                table: "TournamentRules",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRules_Tournament_TournamentId",
                table: "TournamentRules");

            migrationBuilder.DropIndex(
                name: "IX_TournamentRules_TournamentId",
                table: "TournamentRules");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "TournamentRules");
        }
    }
}
