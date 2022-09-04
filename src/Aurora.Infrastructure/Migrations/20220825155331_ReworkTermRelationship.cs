using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class ReworkTermRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestToResult");

            migrationBuilder.AddColumn<Guid>(
                name: "SearchRequestId",
                table: "Result",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "SearchTerm",
                table: "Request",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_Result_SearchRequestId",
                table: "Result",
                column: "SearchRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Request_SearchRequestId",
                table: "Result",
                column: "SearchRequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Result_Request_SearchRequestId",
                table: "Result");

            migrationBuilder.DropIndex(
                name: "IX_Result_SearchRequestId",
                table: "Result");

            migrationBuilder.DropColumn(
                name: "SearchRequestId",
                table: "Result");

            migrationBuilder.AlterColumn<string>(
                name: "SearchTerm",
                table: "Request",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "RequestToResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchResultId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestToResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestToResult_Request_SearchRequestId",
                        column: x => x.SearchRequestId,
                        principalTable: "Request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestToResult_Result_SearchResultId",
                        column: x => x.SearchResultId,
                        principalTable: "Result",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestToResult_SearchRequestId",
                table: "RequestToResult",
                column: "SearchRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToResult_SearchResultId_SearchRequestId",
                table: "RequestToResult",
                columns: new[] { "SearchResultId", "SearchRequestId" },
                unique: true);
        }
    }
}
