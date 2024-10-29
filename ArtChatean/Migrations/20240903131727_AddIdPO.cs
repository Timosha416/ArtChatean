using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtChatean.Migrations
{
    /// <inheritdoc />
    public partial class AddIdPO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PictureOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "PictureOrders");
        }
    }
}
