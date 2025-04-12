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

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLogs_OrderId",
                table: "ReputationLogs",
                column: "OrderId");

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
