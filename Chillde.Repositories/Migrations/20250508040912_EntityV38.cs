using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV38 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "PointChange",
                table: "ReputationLogs",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<float>(
                name: "Value",
                table: "CancellationReasons",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<float>(
                name: "TotalReputation",
                table: "AccountRoles",
                type: "real",
                nullable: false,
                defaultValue: 100f,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 100m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PointChange",
                table: "ReputationLogs",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "CancellationReasons",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalReputation",
                table: "AccountRoles",
                type: "numeric",
                nullable: false,
                defaultValue: 100m,
                oldClrType: typeof(float),
                oldType: "real",
                oldDefaultValue: 100f);
        }
    }
}
