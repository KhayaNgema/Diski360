using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class Re_UpdateEfCoreThing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraints from TournamentMatchResults
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentMatchResults");

            // Drop FK constraints from TournamentFixtures
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures");

            // Drop the existing primary key on TournamentClubs
            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            // Drop the unwanted column if exists (TournamentClubId)
            migrationBuilder.Sql(@"
        IF EXISTS (
            SELECT 1 
            FROM sys.columns 
            WHERE Name = N'TournamentClubId' 
            AND Object_ID = Object_ID(N'TournamentClubs')
        )
        BEGIN
            ALTER TABLE TournamentClubs DROP COLUMN TournamentClubId;
        END
    ");

            // Add composite primary key on ClubId and TournamentId
            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                columns: new[] { "ClubId", "TournamentId" });

            // Recreate FK constraints on TournamentMatchResults
            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatchResults_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentMatchResults",
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

            // Recreate FK constraints on TournamentFixtures
            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_HomeTeamClubId_HomeTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "HomeTeamClubId", "HomeTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentFixtures_TournamentClubs_AwayTeamClubId_AwayTeamTournamentId",
                table: "TournamentFixtures",
                columns: new[] { "AwayTeamClubId", "AwayTeamTournamentId" },
                principalTable: "TournamentClubs",
                principalColumns: new[] { "ClubId", "TournamentId" },
                onDelete: ReferentialAction.NoAction);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop composite PK
            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            // Re-add TournamentClubId column (if needed for rollback)
            migrationBuilder.AddColumn<int>(
                name: "TournamentClubId",
                table: "TournamentClubs",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Set primary key back to TournamentClubId
            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                column: "TournamentClubId");
        }
    }
}
