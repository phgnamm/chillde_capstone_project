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
                name: "FK_RequestDetail_ItemAttributes_ItemAttributeId",
                table: "RequestDetail");

            migrationBuilder.DropTable(
                name: "ItemAttributeValue");

            migrationBuilder.DropIndex(
                name: "IX_RequestDetail_ItemAttributeId",
                table: "RequestDetail");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "ItemAttributes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ItemAttributes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ItemAttributes");

            migrationBuilder.RenameColumn(
                name: "ItemAttributeId",
                table: "RequestDetail",
                newName: "AttributeId");

            migrationBuilder.AddColumn<Guid>(
                name: "AttributeId",
                table: "ItemAttributes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Attributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttributeValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    IntOrder = table.Column<int>(type: "integer", nullable: false),
                    AttributeId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AttributeValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeValue_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestDetail_AttributeId",
                table: "RequestDetail",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_AttributeId",
                table: "ItemAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValue_AttributeId",
                table: "AttributeValue",
                column: "AttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributes_Attributes_AttributeId",
                table: "ItemAttributes",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestDetail_Attributes_AttributeId",
                table: "RequestDetail",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributes_Attributes_AttributeId",
                table: "ItemAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestDetail_Attributes_AttributeId",
                table: "RequestDetail");

            migrationBuilder.DropTable(
                name: "AttributeValue");

            migrationBuilder.DropTable(
                name: "Attributes");

            migrationBuilder.DropIndex(
                name: "IX_RequestDetail_AttributeId",
                table: "RequestDetail");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributes_AttributeId",
                table: "ItemAttributes");

            migrationBuilder.DropColumn(
                name: "AttributeId",
                table: "ItemAttributes");

            migrationBuilder.RenameColumn(
                name: "AttributeId",
                table: "RequestDetail",
                newName: "ItemAttributeId");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "ItemAttributes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ItemAttributes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ItemAttributes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemAttributeValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemAttributeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IntOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributeValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemAttributeValue_ItemAttributes_ItemAttributeId",
                        column: x => x.ItemAttributeId,
                        principalTable: "ItemAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestDetail_ItemAttributeId",
                table: "RequestDetail",
                column: "ItemAttributeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeValue_ItemAttributeId",
                table: "ItemAttributeValue",
                column: "ItemAttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestDetail_ItemAttributes_ItemAttributeId",
                table: "RequestDetail",
                column: "ItemAttributeId",
                principalTable: "ItemAttributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
