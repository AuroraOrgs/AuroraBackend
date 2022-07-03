using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class QueueHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchRequestQueueItem",
                columns: table => new
                {
                    QueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SearchRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueuedTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRequestQueueItem", x => x.QueueId);
                    table.ForeignKey(
                        name: "FK_SearchRequestQueueItem_Request_SearchRequestId",
                        column: x => x.SearchRequestId,
                        principalTable: "Request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequestQueueItem_SearchRequestId",
                table: "SearchRequestQueueItem",
                column: "SearchRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchRequestQueueItem");
        }
    }
}
