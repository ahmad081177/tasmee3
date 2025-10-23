using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranListeningApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeToListeningSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Grade",
                table: "ListeningSessions",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "ListeningSessions");
        }
    }
}
