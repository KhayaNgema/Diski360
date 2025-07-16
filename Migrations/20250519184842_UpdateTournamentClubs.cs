using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTournamentClubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

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

            migrationBuilder.DropIndex(
                name: "IX_TournamentMatchResults_AwayTeamId",
                table: "TournamentMatchResults");

            migrationBuilder.DropIndex(
                name: "IX_TournamentMatchResults_HomeTeamId",
                table: "TournamentMatchResults");

            migrationBuilder.DropIndex(
                name: "IX_TournamentFixtures_AwayTeamId",
                table: "TournamentFixtures");

            migrationBuilder.DropIndex(
                name: "IX_TournamentFixtures_HomeTeamId",
                table: "TournamentFixtures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            migrationBuilder.AddColumn<int>(
                name: "AwayTeamClubId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AwayTeamTournamentId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamClubId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamTournamentId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AwayTeamClubId",
                table: "TournamentFixtures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AwayTeamTournamentId",
                table: "TournamentFixtures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamClubId",
                table: "TournamentFixtures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamTournamentId",
                table: "TournamentFixtures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClubId",
                table: "TournamentClubs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                columns: new[] { "ClubId", "TournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentClubs_TournamentId",
                table: "TournamentClubs",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentClubs_Club_ClubId",
                table: "TournamentClubs",
                column: "ClubId",
                principalTable: "Club",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
