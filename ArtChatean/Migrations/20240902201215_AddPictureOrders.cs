using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtChatean.Migrations
{
    /// <inheritdoc />
    public partial class AddPictureOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pictures_Orders_OrderId",
                table: "Pictures");

            migrationBuilder.DropIndex(
                name: "IX_Pictures_OrderId",
                table: "Pictures");

            migrationBuilder.AddColumn<bool>(
                name: "IsSold",
                table: "Pictures",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PictureOrders",
                columns: table => new
                {
                    PictureId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PictureOrders", x => new { x.PictureId, x.OrderId });
                    table.ForeignKey(
                        name: "FK_PictureOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PictureOrders_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PictureOrders_OrderId",
                table: "PictureOrders",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PictureOrders");

            migrationBuilder.DropColumn(
                name: "IsSold",
                table: "Pictures");

            migrationBuilder.CreateIndex(
                name: "IX_Pictures_OrderId",
                table: "Pictures",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pictures_Orders_OrderId",
                table: "Pictures",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
