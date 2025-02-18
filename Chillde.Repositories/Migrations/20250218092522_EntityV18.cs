using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Requests");

            migrationBuilder.AlterColumn<int>(
                name: "EntityType",
                table: "SystemConfigs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsQuantity",
                table: "PackageFeatures",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionType",
                table: "PackageFeatures",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "OrderInformation",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderInformation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs",
                columns: new[] { "EntityType", "FieldName" });

            migrationBuilder.CreateTable(
                name: "RequestAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentAlt = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestAttachments_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestAttachments_RequestId",
                table: "RequestAttachments",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "IsQuantity",
                table: "PackageFeatures");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "PackageFeatures");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "OrderInformation");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderInformation");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "SystemConfigs",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                table: "SystemConfigs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemConfigs",
                table: "SystemConfigs",
                columns: new[] { "EntityType", "EntityId", "FieldName" });
        }
    }
}
