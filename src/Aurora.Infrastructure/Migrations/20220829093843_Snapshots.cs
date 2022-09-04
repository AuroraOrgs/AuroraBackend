using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class Snapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Result_SearchRequestSnapshot_SearchRequestSnapshotId",
                table: "Result");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchRequestSnapshot_Request_SearchRequestId",
                table: "SearchRequestSnapshot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchRequestSnapshot",
                table: "SearchRequestSnapshot");

            migrationBuilder.RenameTable(
                name: "SearchRequestSnapshot",
                newName: "Snapshots");

            migrationBuilder.RenameIndex(
                name: "IX_SearchRequestSnapshot_SearchRequestId",
                table: "Snapshots",
                newName: "IX_Snapshots_SearchRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Snapshots",
                table: "Snapshots",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Snapshots_SearchRequestSnapshotId",
                table: "Result",
                column: "SearchRequestSnapshotId",
                principalTable: "Snapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Snapshots_Request_SearchRequestId",
                table: "Snapshots",
                column: "SearchRequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Result_Snapshots_SearchRequestSnapshotId",
                table: "Result");

            migrationBuilder.DropForeignKey(
                name: "FK_Snapshots_Request_SearchRequestId",
                table: "Snapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Snapshots",
                table: "Snapshots");

            migrationBuilder.RenameTable(
                name: "Snapshots",
                newName: "SearchRequestSnapshot");

            migrationBuilder.RenameIndex(
                name: "IX_Snapshots_SearchRequestId",
                table: "SearchRequestSnapshot",
                newName: "IX_SearchRequestSnapshot_SearchRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchRequestSnapshot",
                table: "SearchRequestSnapshot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Result_SearchRequestSnapshot_SearchRequestSnapshotId",
                table: "Result",
                column: "SearchRequestSnapshotId",
                principalTable: "SearchRequestSnapshot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchRequestSnapshot_Request_SearchRequestId",
                table: "SearchRequestSnapshot",
                column: "SearchRequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
