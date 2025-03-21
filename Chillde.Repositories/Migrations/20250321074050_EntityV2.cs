using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Requests_RequestId",
                table: "Offers");

            migrationBuilder.AddColumn<int>(
                name: "MaxQuantiy",
                table: "Packages",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestId",
                table: "Offers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Requests_RequestId",
                table: "Offers",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Requests_RequestId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "MaxQuantiy",
                table: "Packages");

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestId",
                table: "Offers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Requests_RequestId",
                table: "Offers",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
