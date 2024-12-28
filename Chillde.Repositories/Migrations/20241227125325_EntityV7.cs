using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeValue_Attributes_AttributeId",
                table: "AttributeValue");

            migrationBuilder.AlterColumn<Guid>(
                name: "AttributeId",
                table: "AttributeValue",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Attributes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeValue_Attributes_AttributeId",
                table: "AttributeValue",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeValue_Attributes_AttributeId",
                table: "AttributeValue");

            migrationBuilder.AlterColumn<Guid>(
                name: "AttributeId",
                table: "AttributeValue",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Attributes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeValue_Attributes_AttributeId",
                table: "AttributeValue",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
