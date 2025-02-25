using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentClubsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentClubs",
                columns: table => new
                {
                    TournamentClubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubDivision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    IsStillActive = table.Column<bool>(type: "bit", nullable: false),
                    ClubManagerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubManagerPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubManagerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentClubs", x => x.TournamentClubId);
                    table.ForeignKey(
                        name: "FK_TournamentClubs_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentClubs_TournamentId",
                table: "TournamentClubs",
                column: "TournamentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentClubs");
        }
    }
}
