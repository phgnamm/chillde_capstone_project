using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCollection_Accounts_AccountId",
                table: "ServiceCollection");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCollection_AccountId",
                table: "ServiceCollection");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ServiceCollection");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "ServiceCollection",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCollection_CreatedById",
                table: "ServiceCollection",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCollection_Accounts_CreatedById",
                table: "ServiceCollection",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCollection_Accounts_CreatedById",
                table: "ServiceCollection");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCollection_CreatedById",
                table: "ServiceCollection");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "ServiceCollection",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "ServiceCollection",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCollection_AccountId",
                table: "ServiceCollection",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCollection_Accounts_AccountId",
                table: "ServiceCollection",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
