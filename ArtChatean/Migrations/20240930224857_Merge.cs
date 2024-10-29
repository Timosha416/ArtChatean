using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtChatean.Migrations
{
    /// <inheritdoc />
    public partial class Merge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArtistId",
                table: "Pictures",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BirthPlace = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DeathPlace = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Quote = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Photo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BigPhoto = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Picture1 = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Picture2 = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Picture3 = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Picture4 = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Picture5 = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Picture6 = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Video = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketTariff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxTicketsPerPerson = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTariff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AvailableTickets = table.Column<int>(type: "INTEGER", nullable: false),
                    ArtistId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Event_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTimeSlot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AvailableTickets = table.Column<int>(type: "INTEGER", nullable: false),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTimeSlot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTimeSlot_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pictures_ArtistId",
                table: "Pictures",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_ArtistId",
                table: "Event",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTimeSlot_EventId",
                table: "EventTimeSlot",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pictures_Artists_ArtistId",
                table: "Pictures",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pictures_Artists_ArtistId",
                table: "Pictures");

            migrationBuilder.DropTable(
                name: "EventTimeSlot");

            migrationBuilder.DropTable(
                name: "TicketTariff");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Pictures_ArtistId",
                table: "Pictures");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "Pictures");
        }
    }
}
