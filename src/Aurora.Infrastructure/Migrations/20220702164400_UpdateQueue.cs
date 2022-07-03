using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class UpdateQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchRequestQueueItem_Request_SearchRequestId",
                table: "SearchRequestQueueItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchRequestQueueItem",
                table: "SearchRequestQueueItem");

            migrationBuilder.RenameTable(
                name: "SearchRequestQueueItem",
                newName: "Queue");

            migrationBuilder.RenameIndex(
                name: "IX_SearchRequestQueueItem_SearchRequestId",
                table: "Queue",
                newName: "IX_Queue_SearchRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queue",
                table: "Queue",
                column: "QueueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Queue_Request_SearchRequestId",
                table: "Queue",
                column: "SearchRequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queue_Request_SearchRequestId",
                table: "Queue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queue",
                table: "Queue");

            migrationBuilder.RenameTable(
                name: "Queue",
                newName: "SearchRequestQueueItem");

            migrationBuilder.RenameIndex(
                name: "IX_Queue_SearchRequestId",
                table: "SearchRequestQueueItem",
                newName: "IX_SearchRequestQueueItem_SearchRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchRequestQueueItem",
                table: "SearchRequestQueueItem",
                column: "QueueId");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchRequestQueueItem_Request_SearchRequestId",
                table: "SearchRequestQueueItem",
                column: "SearchRequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
