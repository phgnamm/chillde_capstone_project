using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationContents_NotificationTypeId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "NotificationTypeId",
                table: "Notifications",
                newName: "NotificationContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_NotificationTypeId",
                table: "Notifications",
                newName: "IX_Notifications_NotificationContentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "Deposits",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationContents_NotificationContentId",
                table: "Notifications",
                column: "NotificationContentId",
                principalTable: "NotificationContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationContents_NotificationContentId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "NotificationContentId",
                table: "Notifications",
                newName: "NotificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_NotificationContentId",
                table: "Notifications",
                newName: "IX_Notifications_NotificationTypeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "Deposits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationContents_NotificationTypeId",
                table: "Notifications",
                column: "NotificationTypeId",
                principalTable: "NotificationContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
