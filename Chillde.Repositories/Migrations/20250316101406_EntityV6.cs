using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Offers_OfferId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_OfferId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "TotalValue",
                table: "Orders",
                newName: "ShippingPrice");

            migrationBuilder.RenameColumn(
                name: "PackagePrice",
                table: "Orders",
                newName: "OriginPrice");

            migrationBuilder.RenameColumn(
                name: "FinalValue",
                table: "Orders",
                newName: "AfterApplyVoucherPrice");

            migrationBuilder.RenameColumn(
                name: "AdminCommission",
                table: "Orders",
                newName: "AdminCommUsedVch");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdminCommDefault",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancellationReasonId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Code",
                table: "Vouchers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CancellationReasonId",
                table: "Orders",
                column: "CancellationReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Code",
                table: "Orders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShipmentCode",
                table: "Orders",
                column: "ShipmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CancellationReasons_CancellationReasonId",
                table: "Orders",
                column: "CancellationReasonId",
                principalTable: "CancellationReasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CancellationReasons_CancellationReasonId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_Code",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CancellationReasonId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Code",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShipmentCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Languages_Code",
                table: "Languages");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "AdminCommDefault",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancellationReasonId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ShippingPrice",
                table: "Orders",
                newName: "TotalValue");

            migrationBuilder.RenameColumn(
                name: "OriginPrice",
                table: "Orders",
                newName: "PackagePrice");

            migrationBuilder.RenameColumn(
                name: "AfterApplyVoucherPrice",
                table: "Orders",
                newName: "FinalValue");

            migrationBuilder.RenameColumn(
                name: "AdminCommUsedVch",
                table: "Orders",
                newName: "AdminCommission");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_OfferId",
                table: "Categories",
                column: "OfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Offers_OfferId",
                table: "Categories",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id");
        }
    }
}
