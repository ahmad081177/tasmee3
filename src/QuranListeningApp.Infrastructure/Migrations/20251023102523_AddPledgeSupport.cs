using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranListeningApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPledgeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PledgeAcceptedDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PledgeText",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PledgeAcceptedDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PledgeText",
                table: "AppSettings");
        }
    }
}
