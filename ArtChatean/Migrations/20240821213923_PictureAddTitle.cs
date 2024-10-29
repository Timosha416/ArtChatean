using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtChatean.Migrations
{
    /// <inheritdoc />
    public partial class PictureAddTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Pictures",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Pictures",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Pictures");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Pictures");
        }
    }
}
