using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FeedbackCount",
                table: "Services",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rate",
                table: "Services",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryRevision",
                table: "Packages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryRevisionPrice",
                table: "Packages",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryTime",
                table: "Packages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SketchRevision",
                table: "Packages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SketchRevisionPrice",
                table: "Packages",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Response",
                table: "Feedbacks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedbackCount",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DeliveryRevision",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeliveryRevisionPrice",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "SketchRevision",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "SketchRevisionPrice",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "Response",
                table: "Feedbacks");
        }
    }
}
