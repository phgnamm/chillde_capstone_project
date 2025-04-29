using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV34 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "SystemConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SystemConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SystemConfigs");
        }
    }
}
