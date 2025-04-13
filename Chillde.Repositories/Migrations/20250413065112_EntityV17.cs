using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherUsageLogs_Accounts_CustomerId",
                table: "VoucherUsageLogs");

            migrationBuilder.DropIndex(
                name: "IX_VoucherUsageLogs_CustomerId",
                table: "VoucherUsageLogs");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "VoucherUsageLogs");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsageLogs_CreatedById",
                table: "VoucherUsageLogs",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherUsageLogs_Accounts_CreatedById",
                table: "VoucherUsageLogs",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherUsageLogs_Accounts_CreatedById",
                table: "VoucherUsageLogs");

            migrationBuilder.DropIndex(
                name: "IX_VoucherUsageLogs_CreatedById",
                table: "VoucherUsageLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "VoucherUsageLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsageLogs_CustomerId",
                table: "VoucherUsageLogs",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherUsageLogs_Accounts_CustomerId",
                table: "VoucherUsageLogs",
                column: "CustomerId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
