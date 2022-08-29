using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class RenameRequestToOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queue_Request_SearchRequestId",
                table: "Queue");

            migrationBuilder.DropForeignKey(
                name: "FK_Result_Snapshots_SearchRequestSnapshotId",
                table: "Result");

            migrationBuilder.DropForeignKey(
                name: "FK_Snapshots_Request_SearchRequestId",
                table: "Snapshots");

            migrationBuilder.DropTable(
                name: "Request");

            migrationBuilder.RenameColumn(
                name: "SearchRequestId",
                table: "Snapshots",
                newName: "SearchOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_Snapshots_SearchRequestId",
                table: "Snapshots",
                newName: "IX_Snapshots_SearchOptionId");

            migrationBuilder.RenameColumn(
                name: "SearchRequestSnapshotId",
                table: "Result",
                newName: "SearchOptionSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_Result_SearchRequestSnapshotId",
                table: "Result",
                newName: "IX_Result_SearchOptionSnapshotId");

            migrationBuilder.RenameColumn(
                name: "SearchRequestId",
                table: "Queue",
                newName: "SearchOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_Queue_SearchRequestId",
                table: "Queue",
                newName: "IX_Queue_SearchOptionId");

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchTerm = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    OccurredCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Options_SearchTerm_Website_ContentType",
                table: "Options",
                columns: new[] { "SearchTerm", "Website", "ContentType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Queue_Options_SearchOptionId",
                table: "Queue",
                column: "SearchOptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Snapshots_SearchOptionSnapshotId",
                table: "Result",
                column: "SearchOptionSnapshotId",
                principalTable: "Snapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Snapshots_Options_SearchOptionId",
                table: "Snapshots",
                column: "SearchOptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queue_Options_SearchOptionId",
                table: "Queue");

            migrationBuilder.DropForeignKey(
                name: "FK_Result_Snapshots_SearchOptionSnapshotId",
                table: "Result");

            migrationBuilder.DropForeignKey(
                name: "FK_Snapshots_Options_SearchOptionId",
                table: "Snapshots");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.RenameColumn(
                name: "SearchOptionId",
                table: "Snapshots",
                newName: "SearchRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Snapshots_SearchOptionId",
                table: "Snapshots",
                newName: "IX_Snapshots_SearchRequestId");

            migrationBuilder.RenameColumn(
                name: "SearchOptionSnapshotId",
                table: "Result",
                newName: "SearchRequestSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_Result_SearchOptionSnapshotId",
                table: "Result",
                newName: "IX_Result_SearchRequestSnapshotId");

            migrationBuilder.RenameColumn(
                name: "SearchOptionId",
                table: "Queue",
                newName: "SearchRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Queue_SearchOptionId",
                table: "Queue",
                newName: "IX_Queue_SearchRequestId");

            migrationBuilder.CreateTable(
                name: "Request",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    OccurredCount = table.Column<int>(type: "integer", nullable: false),
                    SearchTerm = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Request_SearchTerm_Website_ContentType",
                table: "Request",
                columns: new[] { "SearchTerm", "Website", "ContentType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Queue_Request_SearchRequestId",
                table: "Queue",
                column: "SearchRequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
