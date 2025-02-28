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
                name: "FK_Orders_Shipment_ShipmentId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShipmentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "FromDistrictId",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "FromWardCode",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "ToDistrictId",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "ToWardCode",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Shipment",
                newName: "StatusId");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Shipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedDeliverTime",
                table: "Shipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedPickTime",
                table: "Shipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Shipment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFee",
                table: "Shipment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Shipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerId",
                table: "Shipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingId",
                table: "Shipment",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductShipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ProductCode = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductShipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductShipment_Shipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_OrderId",
                table: "Shipment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductShipment_ShipmentId",
                table: "ProductShipment",
                column: "ShipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_Orders_OrderId",
                table: "Shipment",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_Orders_OrderId",
                table: "Shipment");

            migrationBuilder.DropTable(
                name: "ProductShipment");

            migrationBuilder.DropIndex(
                name: "IX_Shipment_OrderId",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliverTime",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "EstimatedPickTime",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "InsuranceFee",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Shipment");

            migrationBuilder.DropColumn(
                name: "TrackingId",
                table: "Shipment");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Shipment",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Shipment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromDistrictId",
                table: "Shipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromWardCode",
                table: "Shipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Shipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Length",
                table: "Shipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Shipment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToDistrictId",
                table: "Shipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToWardCode",
                table: "Shipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Shipment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Shipment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShipmentId",
                table: "Orders",
                column: "ShipmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Shipment_ShipmentId",
                table: "Orders",
                column: "ShipmentId",
                principalTable: "Shipment",
                principalColumn: "Id");
        }
    }
}
