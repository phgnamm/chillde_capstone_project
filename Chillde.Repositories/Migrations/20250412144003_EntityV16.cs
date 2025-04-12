using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedDays",
                table: "OrderTrackings");

            migrationBuilder.RenameColumn(
                name: "CancelOrderReason",
                table: "Orders",
                newName: "SystemCancelReason");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Vouchers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "ReputationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeadlineSent",
                table: "OrderTrackings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReminder50Sent",
                table: "OrderTrackings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReminder80Sent",
                table: "OrderTrackings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WithBalance",
                table: "Orders",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Deposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deposits_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLogs_OrderId",
                table: "ReputationLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_WalletId",
                table: "Deposits",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReputationLogs_Orders_OrderId",
                table: "ReputationLogs",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReputationLogs_Orders_OrderId",
                table: "ReputationLogs");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLogs_OrderId",
                table: "ReputationLogs");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ReputationLogs");

            migrationBuilder.DropColumn(
                name: "IsDeadlineSent",
                table: "OrderTrackings");

            migrationBuilder.DropColumn(
                name: "IsReminder50Sent",
                table: "OrderTrackings");

            migrationBuilder.DropColumn(
                name: "IsReminder80Sent",
                table: "OrderTrackings");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WithBalance",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "SystemCancelReason",
                table: "Orders",
                newName: "CancelOrderReason");

            migrationBuilder.AddColumn<int>(
                name: "ExtendedDays",
                table: "OrderTrackings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
