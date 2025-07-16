using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class DropTournamentIdMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentFixtures",
                columns: table => new
                {
                    FixtureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    KickOffDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KickOffTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FixtureRound = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FixtureStatus = table.Column<int>(type: "int", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentFixtures", x => x.FixtureId);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_TournamentClubs_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "TournamentClubs",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_TournamentClubs_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "TournamentClubs",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TournamentMatchResults",
                columns: table => new
                {
                    ResultsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    TournamentFixtureFixtureId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamScore = table.Column<int>(type: "int", nullable: false),
                    AwayTeamScore = table.Column<int>(type: "int", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatchTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentMatchResults", x => x.ResultsId);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_TournamentFixtures_TournamentFixtureFixtureId",
                        column: x => x.TournamentFixtureFixtureId,
                        principalTable: "TournamentFixtures",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TournamentMatchResults_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_AwayTeamId",
                table: "TournamentFixtures",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_CreatedById",
                table: "TournamentFixtures",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_HomeTeamId",
                table: "TournamentFixtures",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_ModifiedById",
                table: "TournamentFixtures",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_TournamentId",
                table: "TournamentFixtures",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_CreatedById",
                table: "TournamentMatchResults",
                column: "CreatedById");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentMatchResults");

            migrationBuilder.DropTable(
                name: "TournamentFixtures");
        }
    }
}
