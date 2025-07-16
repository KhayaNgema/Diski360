using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    public partial class Checcc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys that reference TournamentClubs(ClubId)
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

            // Drop primary key on TournamentClubs
            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            // Add temporary column ClubId_temp
            migrationBuilder.Sql("ALTER TABLE TournamentClubs ADD ClubId_temp int NOT NULL DEFAULT(0);");

            // Copy data from ClubId to ClubId_temp
            migrationBuilder.Sql("UPDATE TournamentClubs SET ClubId_temp = ClubId;");

            // Drop the old ClubId column
            migrationBuilder.Sql("ALTER TABLE TournamentClubs DROP COLUMN ClubId;");

            // Rename ClubId_temp to ClubId
            migrationBuilder.Sql("EXEC sp_rename 'TournamentClubs.ClubId_temp', 'ClubId', 'COLUMN';");

            // Recreate primary key with new ClubId column
            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                columns: new[] { "ClubId", "TournamentId" });

            // Recreate foreign keys
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys
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

            // Drop primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs");

            // Add old ClubId column back with identity if needed (adjust as per original)
            migrationBuilder.Sql("ALTER TABLE TournamentClubs ADD ClubId_temp int IDENTITY(1,1) NOT NULL;");

            // Copy data back (you might lose original values here, so be cautious)
            migrationBuilder.Sql("UPDATE TournamentClubs SET ClubId_temp = ClubId;");

            // Drop current ClubId column
            migrationBuilder.Sql("ALTER TABLE TournamentClubs DROP COLUMN ClubId;");

            // Rename ClubId_temp back to ClubId
            migrationBuilder.Sql("EXEC sp_rename 'TournamentClubs.ClubId_temp', 'ClubId', 'COLUMN';");

            // Re-add primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentClubs",
                table: "TournamentClubs",
                column: "ClubId");

            // Re-add foreign keys as they were originally (adjust columns and tables accordingly)
            // ...
        }
    }
}
