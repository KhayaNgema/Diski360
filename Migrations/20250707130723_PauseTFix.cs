using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class PauseTFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentFixtures_TournamentFixtureFixtureId",
                table: "TournamentMatchResults");

            migrationBuilder.DropTable(
                name: "TournamentFixtures");

            migrationBuilder.DropIndex(
                name: "IX_TournamentMatchResults_TournamentFixtureFixtureId",
                table: "TournamentMatchResults");

            migrationBuilder.DropColumn(
                name: "FixtureId",
                table: "TournamentMatchResults");

            migrationBuilder.DropColumn(
                name: "TournamentFixtureFixtureId",
                table: "TournamentMatchResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixtureId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TournamentFixtureFixtureId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TournamentFixtures",
                columns: table => new
                {
                    FixtureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwayTeamClubId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeTeamClubId = table.Column<int>(type: "int", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamTournamentId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamTournamentId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FixtureRound = table.Column<int>(type: "int", nullable: false),
                    FixtureStatus = table.Column<int>(type: "int", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    KickOffDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KickOffTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentFixtures", x => x.FixtureId);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                        columns: x => new { x.AwayTeamClubId, x.AwayTeamTournamentId },
                        principalTable: "TournamentClubs",
                        principalColumns: new[] { "ClubId", "TournamentId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                        columns: x => new { x.HomeTeamClubId, x.HomeTeamTournamentId },
                        principalTable: "TournamentClubs",
                        principalColumns: new[] { "ClubId", "TournamentId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentFixtures_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_TournamentFixtureFixtureId",
                table: "TournamentMatchResults",
                column: "TournamentFixtureFixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_CreatedById",
                table: "TournamentFixtures",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_ModifiedById",
                table: "TournamentFixtures",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_TournamentId",
                table: "TournamentFixtures",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentFixtures_TournamentFixtureFixtureId",
                table: "TournamentMatchResults",
                column: "TournamentFixtureFixtureId",
                principalTable: "TournamentFixtures",
                principalColumn: "FixtureId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
