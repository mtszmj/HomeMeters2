using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeMeters2.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPlaceRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Places",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Places_OwnerId",
                table: "Places",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Places_AspNetUsers_OwnerId",
                table: "Places",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Places_AspNetUsers_OwnerId",
                table: "Places");

            migrationBuilder.DropIndex(
                name: "IX_Places_OwnerId",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Places");
        }
    }
}
