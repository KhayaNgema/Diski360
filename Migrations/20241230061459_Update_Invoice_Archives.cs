using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class Update_Invoice_Archives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Invoices");

            migrationBuilder.CreateTable(
                name: "Invoices_Archives",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    FineId = table.Column<int>(type: "int", nullable: true),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    InvoiceTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsEmailed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices_Archives", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_Archives_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Archives_Fines_FineId",
                        column: x => x.FineId,
                        principalTable: "Fines",
                        principalColumn: "FineId");
                    table.ForeignKey(
                        name: "FK_Invoices_Archives_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                    table.ForeignKey(
                        name: "FK_Invoices_Archives_Transfer_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfer",
                        principalColumn: "TransferId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Archives_CreatedById",
                table: "Invoices_Archives",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Archives_FineId",
                table: "Invoices_Archives",
                column: "FineId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Archives_PaymentId",
                table: "Invoices_Archives",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Archives_TransferId",
                table: "Invoices_Archives",
                column: "TransferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices_Archives");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Invoices",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");
        }
    }
}
