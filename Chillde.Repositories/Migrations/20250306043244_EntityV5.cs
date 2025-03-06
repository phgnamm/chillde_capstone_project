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
                name: "FK_CancellationReasons_Roles_RoleId",
                table: "CancellationReasons");

            migrationBuilder.DropForeignKey(
                name: "FK_ReputationLogs_Accounts_AccountId",
                table: "ReputationLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs");

            migrationBuilder.DropIndex(
                name: "IX_CancellationReasons_RoleId",
                table: "CancellationReasons");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "CancellationReasons");

            migrationBuilder.DropColumn(
                name: "ReputationPoints",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "ReputationLogs",
                newName: "AccountRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_ReputationLogs_AccountId",
                table: "ReputationLogs",
                newName: "IX_ReputationLogs_AccountRoleId");

            migrationBuilder.RenameColumn(
                name: "YearlyAutoCancels",
                table: "Accounts",
                newName: "YMonthlyAutoCancels");

            migrationBuilder.AlterColumn<string>(
                name: "FieldName",
                table: "SystemConfigs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SystemConfigs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AccountRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReputation",
                table: "AccountRoles",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReputationLogs_AccountRoles_AccountRoleId",
                table: "ReputationLogs",
                column: "AccountRoleId",
                principalTable: "AccountRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReputationLogs_AccountRoles_AccountRoleId",
                table: "ReputationLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AccountRoles");

            migrationBuilder.DropColumn(
                name: "TotalReputation",
                table: "AccountRoles");

            migrationBuilder.RenameColumn(
                name: "AccountRoleId",
                table: "ReputationLogs",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_ReputationLogs_AccountRoleId",
                table: "ReputationLogs",
                newName: "IX_ReputationLogs_AccountId");

            migrationBuilder.RenameColumn(
                name: "YMonthlyAutoCancels",
                table: "Accounts",
                newName: "YearlyAutoCancels");

            migrationBuilder.AlterColumn<string>(
                name: "FieldName",
                table: "SystemConfigs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "CancellationReasons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReputationPoints",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs",
                columns: new[] { "EntityType", "FieldName" });

            migrationBuilder.CreateIndex(
                name: "IX_CancellationReasons_RoleId",
                table: "CancellationReasons",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CancellationReasons_Roles_RoleId",
                table: "CancellationReasons",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReputationLogs_Accounts_AccountId",
                table: "ReputationLogs",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
