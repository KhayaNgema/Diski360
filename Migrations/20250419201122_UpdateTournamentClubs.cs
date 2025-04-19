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
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_CreatedById",
                table: "TournamentClubs");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_ModifiedById",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "HasPaid",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "IsStillActive",
                table: "TournamentClubs");

            migrationBuilder.RenameColumn(
                name: "ClubDivision",
                table: "TournamentClubs",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "TournamentClubId",
                table: "TournamentClubs",
                newName: "ClubId");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedById",
                table: "TournamentClubs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "TournamentClubs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClubAbbr",
                table: "TournamentClubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClubBadge",
                table: "TournamentClubs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClubHistory",
                table: "TournamentClubs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClubLocation",
                table: "TournamentClubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClubSummary",
                table: "TournamentClubs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "TournamentClubs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasJoined",
                table: "TournamentClubs",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEliminated",
                table: "TournamentClubs",
                type: "bit",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentClubs_DivisionId",
                table: "TournamentClubs",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_CreatedById",
                table: "TournamentClubs",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_ModifiedById",
                table: "TournamentClubs",
                column: "ModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentClubs_Divisions_DivisionId",
                table: "TournamentClubs",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_CreatedById",
                table: "TournamentClubs");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_ModifiedById",
                table: "TournamentClubs");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentClubs_Divisions_DivisionId",
                table: "TournamentClubs");

            migrationBuilder.DropIndex(
                name: "IX_TournamentClubs_DivisionId",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "ClubAbbr",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "ClubBadge",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "ClubHistory",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "ClubLocation",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "ClubSummary",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "HasJoined",
                table: "TournamentClubs");

            migrationBuilder.DropColumn(
                name: "IsEliminated",
                table: "TournamentClubs");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "TournamentClubs",
                newName: "ClubDivision");

            migrationBuilder.RenameColumn(
                name: "ClubId",
                table: "TournamentClubs",
                newName: "TournamentClubId");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedById",
                table: "TournamentClubs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "TournamentClubs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "HasPaid",
                table: "TournamentClubs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStillActive",
                table: "TournamentClubs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_CreatedById",
                table: "TournamentClubs",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentClubs_AspNetUsers_ModifiedById",
                table: "TournamentClubs",
                column: "ModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
