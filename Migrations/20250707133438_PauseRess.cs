using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class PauseRess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentMatchResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentMatchResults",
                columns: table => new
                {
                    ResultsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwayTeamClubId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeTeamClubId = table.Column<int>(type: "int", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TournamentFixtureFixtureId = table.Column<int>(type: "int", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamTournamentId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamTournamentId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamScore = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamScore = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatchTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentMatchResults", x => x.ResultsId);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                        columns: x => new { x.AwayTeamClubId, x.AwayTeamTournamentId },
                        principalTable: "TournamentClubs",
                        principalColumns: new[] { "ClubId", "TournamentId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                        columns: x => new { x.HomeTeamClubId, x.HomeTeamTournamentId },
                        principalTable: "TournamentClubs",
                        principalColumns: new[] { "ClubId", "TournamentId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_TournamentFixtures_TournamentFixtureFixtureId",
                        column: x => x.TournamentFixtureFixtureId,
                        principalTable: "TournamentFixtures",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_CreatedById",
                table: "TournamentMatchResults",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_ModifiedById",
                table: "TournamentMatchResults",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_TournamentFixtureFixtureId",
                table: "TournamentMatchResults",
                column: "TournamentFixtureFixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_TournamentId",
                table: "TournamentMatchResults",
                column: "TournamentId");
        }
    }
}
