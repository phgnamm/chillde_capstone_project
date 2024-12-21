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
            migrationBuilder.DropPrimaryKey(
                name: "PK_Translations",
                table: "Translations");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "OrderInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "Feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Translations",
                table: "Translations",
                columns: new[] { "EntityType", "EntityId", "FieldName", "LanguageId" });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CreatedById",
                table: "Feedbacks",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Accounts_CreatedById",
                table: "Feedbacks",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Accounts_CreatedById",
                table: "Feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Translations",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_CreatedById",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "OrderInformation");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "Feedbacks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Translations",
                table: "Translations",
                column: "Id");
        }
    }
}
