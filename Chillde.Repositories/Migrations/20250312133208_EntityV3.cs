using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Services_ServiceId",
                table: "Offers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "Offers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Services_ServiceId",
                table: "Offers",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Services_ServiceId",
                table: "Offers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "Offers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Services_ServiceId",
                table: "Offers",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
