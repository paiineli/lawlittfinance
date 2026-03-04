using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawllitFinance.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeAndFontSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "dark");

            migrationBuilder.AddColumn<string>(
                name: "FontSize",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "normal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FontSize",
                table: "Users");
        }
    }
}
