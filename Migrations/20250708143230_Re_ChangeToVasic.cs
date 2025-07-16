using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class Re_ChangeToVasic : Migration
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


            migrationBuilder.RenameColumn(
                name: "HomeTeamId",
                table: "TournamentMatchResults",
                newName: "HomeTeamTournamentId");

            migrationBuilder.RenameColumn(
                name: "AwayTeamId",
                table: "TournamentMatchResults",
                newName: "HomeTeamClubId");

            migrationBuilder.RenameColumn(
                name: "HomeTeamId",
                table: "TournamentFixtures",
                newName: "HomeTeamTournamentId");

            migrationBuilder.RenameColumn(
                name: "AwayTeamId",
                table: "TournamentFixtures",
                newName: "HomeTeamClubId");

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

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "TournamentFixtures",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentMatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults");

            migrationBuilder.DropIndex(
                name: "IX_TournamentMatchResults_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentMatchResults");

            migrationBuilder.DropIndex(
                name: "IX_TournamentMatchResults_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults");

            migrationBuilder.DropIndex(
                name: "IX_TournamentFixtures_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures");

            migrationBuilder.DropIndex(
                name: "IX_TournamentFixtures_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "AwayTeamClubId",
                table: "TournamentMatchResults");

            migrationBuilder.DropColumn(
                name: "AwayTeamTournamentId",
                table: "TournamentMatchResults");

            migrationBuilder.DropColumn(
                name: "AwayTeamClubId",
                table: "TournamentFixtures");

            migrationBuilder.DropColumn(
                name: "AwayTeamTournamentId",
                table: "TournamentFixtures");

            migrationBuilder.RenameColumn(
                name: "HomeTeamTournamentId",
                table: "TournamentMatchResults",
                newName: "HomeTeamId");

            migrationBuilder.RenameColumn(
                name: "HomeTeamClubId",
                table: "TournamentMatchResults",
                newName: "AwayTeamId");

            migrationBuilder.RenameColumn(
                name: "HomeTeamTournamentId",
                table: "TournamentFixtures",
                newName: "HomeTeamId");

            migrationBuilder.RenameColumn(
                name: "HomeTeamClubId",
                table: "TournamentFixtures",
                newName: "AwayTeamId");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "TournamentFixtures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                column: "ClubId");

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
                name: "IX_TournamentClubs_ClubId_TournamentId",
                table: "TournamentClubs",
                columns: new[] { "ClubId", "TournamentId" },
                unique: true);

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
    }
}
