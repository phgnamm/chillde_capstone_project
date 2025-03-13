using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Features_Offers_OfferId",
                table: "Features");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Services_ServiceId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Features_OfferId",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "ResponseTime",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "SketchRevision",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "Features");

            migrationBuilder.AddColumn<float>(
                name: "MaxWeight",
                table: "Services",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MinWeight",
                table: "Services",
                type: "real",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "Packages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferId",
                table: "Packages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxQuantity",
                table: "PackageFeatures",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_OfferId",
                table: "Packages",
                column: "OfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Offers_OfferId",
                table: "Packages",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Services_ServiceId",
                table: "Packages",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Offers_OfferId",
                table: "Packages");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Services_ServiceId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_OfferId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "MaxWeight",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "MinWeight",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "Packages");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "Packages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxQuantity",
                table: "PackageFeatures",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryTime",
                table: "Offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Offers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ResponseTime",
                table: "Offers",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SketchRevision",
                table: "Offers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfferId",
                table: "Features",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Features_OfferId",
                table: "Features",
                column: "OfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_Features_Offers_OfferId",
                table: "Features",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Services_ServiceId",
                table: "Packages",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
