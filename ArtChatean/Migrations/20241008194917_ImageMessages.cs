using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtChatean.Migrations
{
    /// <inheritdoc />
    public partial class ImageMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MessageText",
                table: "ChatMessages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<byte[]>(
                name: "MessageImage",
                table: "ChatMessages",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageImage",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "MessageText",
                table: "ChatMessages",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
