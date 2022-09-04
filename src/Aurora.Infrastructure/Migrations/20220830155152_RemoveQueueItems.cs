using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class RemoveQueueItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Queue");

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "Snapshots",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "Snapshots");

            migrationBuilder.CreateTable(
                name: "Queue",
                columns: table => new
                {
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    QueuedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queue", x => x.QueueId);
                    table.ForeignKey(
                        name: "FK_Queue_Options_SearchOptionId",
                        column: x => x.SearchOptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Queue_SearchOptionId",
                table: "Queue",
                column: "SearchOptionId");
        }
    }
}
