using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class CheckPendingMigrations : Migration
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

            migrationBuilder.DropTable(
                name: "TournamentClubs");

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

            migrationBuilder.DropColumn(
                name: "AwayTeamId",
                table: "TournamentMatchResults");

            migrationBuilder.DropColumn(
                name: "HomeTeamId",
                table: "TournamentMatchResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayTeamId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamId",
                table: "TournamentMatchResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TournamentClubs",
                columns: table => new
                {
                    ClubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    ClubAbbr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubBadge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClubHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClubLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubManagerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubManagerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubManagerPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasJoined = table.Column<bool>(type: "bit", nullable: true),
                    IsEliminated = table.Column<bool>(type: "bit", nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentClubs", x => x.ClubId);
                    table.ForeignKey(
                        name: "FK_TournamentClubs_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentClubs_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentClubs_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_TournamentClubs_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_AwayTeamId",
                table: "TournamentMatchResults",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchResults_HomeTeamId",
                table: "TournamentMatchResults",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_AwayTeamId",
                table: "TournamentFixtures",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentFixtures_HomeTeamId",
                table: "TournamentFixtures",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentClubs_CreatedById",
                table: "TournamentClubs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentClubs_DivisionId",
                table: "TournamentClubs",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentClubs_ModifiedById",
                table: "TournamentClubs",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentClubs_TournamentId",
                table: "TournamentClubs",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamId",
                table: "TournamentFixtures",
                column: "AwayTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamId",
                table: "TournamentFixtures",
                column: "HomeTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamId",
                table: "TournamentMatchResults",
                column: "AwayTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamId",
                table: "TournamentMatchResults",
                column: "HomeTeamId",
                principalTable: "TournamentClubs",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
