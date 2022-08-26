using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class AddSnapshoting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Result_Request_SearchRequestId",
                table: "Result");

            migrationBuilder.RenameColumn(
                name: "SearchRequestId",
                table: "Result",
                newName: "SearchRequestSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_Result_SearchRequestId",
                table: "Result",
                newName: "IX_Result_SearchRequestSnapshotId");

            migrationBuilder.CreateTable(
                name: "SearchRequestSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SearchRequestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRequestSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchRequestSnapshot_Request_SearchRequestId",
                        column: x => x.SearchRequestId,
                        principalTable: "Request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequestSnapshot_SearchRequestId",
                table: "SearchRequestSnapshot",
                column: "SearchRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Result_SearchRequestSnapshot_SearchRequestSnapshotId",
                table: "Result",
                column: "SearchRequestSnapshotId",
                principalTable: "SearchRequestSnapshot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Result_SearchRequestSnapshot_SearchRequestSnapshotId",
                table: "Result");

            migrationBuilder.DropTable(
                name: "SearchRequestSnapshot");

            migrationBuilder.RenameColumn(
                name: "SearchRequestSnapshotId",
                table: "Result",
                newName: "SearchRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Result_SearchRequestSnapshotId",
                table: "Result",
                newName: "IX_Result_SearchRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Request_SearchRequestId",
                table: "Result",
                column: "SearchRequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
