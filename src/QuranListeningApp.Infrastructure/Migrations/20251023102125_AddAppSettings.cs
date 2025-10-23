using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranListeningApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SchoolNameArabic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchoolLogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
