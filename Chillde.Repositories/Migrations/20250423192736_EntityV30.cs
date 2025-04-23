using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
              name: "DateTimeCreationVoucher",
              table: "Orders",
              newName: "DateTimeCreateVoucher");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
              name: "DateTimeCreateVoucher",
              table: "Orders",
              newName: "DateTimeCreationVoucher");
        }
    }
}
