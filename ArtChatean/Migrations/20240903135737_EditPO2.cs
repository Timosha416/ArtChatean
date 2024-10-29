using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtChatean.Migrations
{
    /// <inheritdoc />
    public partial class EditPO2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PictureOrders_Orders_OrderId",
                table: "PictureOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PictureOrders_Pictures_PictureId",
                table: "PictureOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PictureOrders",
                table: "PictureOrders");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PictureOrders",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PictureOrders",
                table: "PictureOrders",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PictureOrders_PictureId",
                table: "PictureOrders",
                column: "PictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PictureOrders_Orders_OrderId",
                table: "PictureOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PictureOrders_Pictures_PictureId",
                table: "PictureOrders",
                column: "PictureId",
                principalTable: "Pictures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PictureOrders_Orders_OrderId",
                table: "PictureOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PictureOrders_Pictures_PictureId",
                table: "PictureOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PictureOrders",
                table: "PictureOrders");

            migrationBuilder.DropIndex(
                name: "IX_PictureOrders_PictureId",
                table: "PictureOrders");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PictureOrders",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PictureOrders",
                table: "PictureOrders",
                columns: new[] { "PictureId", "OrderId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PictureOrders_Orders_OrderId",
                table: "PictureOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PictureOrders_Pictures_PictureId",
                table: "PictureOrders",
                column: "PictureId",
                principalTable: "Pictures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
