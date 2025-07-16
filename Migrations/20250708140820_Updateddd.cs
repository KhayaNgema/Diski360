using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class Updateddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamId",
                table: "TournamentFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamId",
                table: "TournamentFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamId",
                table: "TournamentMatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamId",
                table: "TournamentMatchResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamId",
                table: "TournamentFixtures",
                column: "AwayTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamId",
                table: "TournamentFixtures",
                column: "HomeTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamId",
                table: "TournamentMatchResults",
                column: "AwayTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamId",
                table: "TournamentMatchResults",
                column: "HomeTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamId",
                table: "TournamentFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamId",
                table: "TournamentFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamId",
                table: "TournamentMatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamId",
                table: "TournamentMatchResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            migrationBuilder.AddColumn<int>(
                name: "TournamentClubId",
                table: "TournamentClubs",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                column: "TournamentClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamId",
                table: "TournamentFixtures",
                column: "AwayTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "TournamentClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamId",
                table: "TournamentFixtures",
                column: "HomeTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "TournamentClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamId",
                table: "TournamentMatchResults",
                column: "AwayTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "TournamentClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamId",
                table: "TournamentMatchResults",
                column: "HomeTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "TournamentClubId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
