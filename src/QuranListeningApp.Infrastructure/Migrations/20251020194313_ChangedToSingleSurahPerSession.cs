using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranListeningApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedToSingleSurahPerSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromSurahNumber",
                table: "ListeningSessions");

            migrationBuilder.RenameColumn(
                name: "ToSurahNumber",
                table: "ListeningSessions",
                newName: "SurahNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SurahNumber",
                table: "ListeningSessions",
                newName: "ToSurahNumber");

            migrationBuilder.AddColumn<int>(
                name: "FromSurahNumber",
                table: "ListeningSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
