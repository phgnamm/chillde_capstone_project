using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Accounts_AccountId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_NotificationContent_NotificationTypeId",
                table: "Notification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationContent",
                table: "NotificationContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.RenameTable(
                name: "NotificationContent",
                newName: "NotificationContents");

            migrationBuilder.RenameTable(
                name: "Notification",
                newName: "Notifications");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_NotificationTypeId",
                table: "Notifications",
                newName: "IX_Notifications_NotificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_AccountId",
                table: "Notifications",
                newName: "IX_Notifications_AccountId");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "NotificationContents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "NotificationContents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationContents",
                table: "NotificationContents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationContents_Code",
                table: "NotificationContents",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Accounts_AccountId",
                table: "Notifications",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationContents_NotificationTypeId",
                table: "Notifications",
                column: "NotificationTypeId",
                principalTable: "NotificationContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Accounts_AccountId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationContents_NotificationTypeId",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationContents",
                table: "NotificationContents");

            migrationBuilder.DropIndex(
                name: "IX_NotificationContents_Code",
                table: "NotificationContents");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "NotificationContents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "NotificationContents");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notification");

            migrationBuilder.RenameTable(
                name: "NotificationContents",
                newName: "NotificationContent");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_NotificationTypeId",
                table: "Notification",
                newName: "IX_Notification_NotificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_AccountId",
                table: "Notification",
                newName: "IX_Notification_AccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationContent",
                table: "NotificationContent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Accounts_AccountId",
                table: "Notification",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_NotificationContent_NotificationTypeId",
                table: "Notification",
                column: "NotificationTypeId",
                principalTable: "NotificationContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
