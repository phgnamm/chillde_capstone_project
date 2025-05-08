using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV36 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PointChange",
                table: "ReputationLogs",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalReputation",
                table: "AccountRoles",
                type: "numeric",
                nullable: false,
                defaultValue: 100m,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PointChange",
                table: "ReputationLogs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "TotalReputation",
                table: "AccountRoles",
                type: "integer",
                nullable: false,
                defaultValue: 100,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 100m);
        }
    }
}
