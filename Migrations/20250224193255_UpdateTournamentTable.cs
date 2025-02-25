using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTournamentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TournamentOrgarnizer",
                table: "Tournament");

            migrationBuilder.DropColumn(
                name: "TournamentRules",
                table: "Tournament");

            migrationBuilder.RenameColumn(
                name: "TournamentType",
                table: "Tournament",
                newName: "TournamentImage");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Tournament",
                newName: "JoiningDueDate");

            migrationBuilder.AlterColumn<double>(
                name: "JoiningFee",
                table: "Tournament",
                type: "float",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfTeams",
                table: "Tournament",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorDetails",
                table: "Tournament",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorName",
                table: "Tournament",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TournamentRules",
                columns: table => new
                {
                    RuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentRules", x => x.RuleId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentRules");

            migrationBuilder.DropColumn(
                name: "NumberOfTeams",
                table: "Tournament");

            migrationBuilder.DropColumn(
                name: "SponsorDetails",
                table: "Tournament");

            migrationBuilder.DropColumn(
                name: "SponsorName",
                table: "Tournament");

            migrationBuilder.RenameColumn(
                name: "TournamentImage",
                table: "Tournament",
                newName: "TournamentType");

            migrationBuilder.RenameColumn(
                name: "JoiningDueDate",
                table: "Tournament",
                newName: "EndDate");

            migrationBuilder.AlterColumn<string>(
                name: "JoiningFee",
                table: "Tournament",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TournamentOrgarnizer",
                table: "Tournament",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TournamentRules",
                table: "Tournament",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
