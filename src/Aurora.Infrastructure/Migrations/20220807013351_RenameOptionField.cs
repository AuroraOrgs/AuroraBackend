using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    public partial class RenameOptionField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentOption",
                table: "Request",
                newName: "ContentType");

            migrationBuilder.RenameIndex(
                name: "IX_Request_SearchTerm_Website_ContentOption",
                table: "Request",
                newName: "IX_Request_SearchTerm_Website_ContentType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Request",
                newName: "ContentOption");

            migrationBuilder.RenameIndex(
                name: "IX_Request_SearchTerm_Website_ContentType",
                table: "Request",
                newName: "IX_Request_SearchTerm_Website_ContentOption");
        }
    }
}
