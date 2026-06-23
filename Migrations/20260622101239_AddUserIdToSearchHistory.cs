using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietNhatHospital.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToSearchHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SearchHistories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchHistories_UserId",
                table: "SearchHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchHistories_AspNetUsers_UserId",
                table: "SearchHistories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchHistories_AspNetUsers_UserId",
                table: "SearchHistories");

            migrationBuilder.DropIndex(
                name: "IX_SearchHistories_UserId",
                table: "SearchHistories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SearchHistories");
        }
    }
}
