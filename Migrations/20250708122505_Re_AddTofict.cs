using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class Re_AddTofict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {



        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentFixtures_FixtureId",
                table: "TournamentMatchResults");

            migrationBuilder.DropIndex(
                name: "IX_TournamentMatchResults_FixtureId",
                table: "TournamentMatchResults");

            migrationBuilder.AddColumn<int>(
                name: "TournamentFixtureFixtureId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_TournamentFixtureFixtureId",
                table: "TournamentMatchResults",
                column: "TournamentFixtureFixtureId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentFixtures_TournamentFixtureFixtureId",
                table: "TournamentMatchResults",
                column: "TournamentFixtureFixtureId",
                principalTable: "TournamentFixtures",
                principalColumn: "FixtureId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
