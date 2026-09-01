using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class Notification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_UploadedByMemberId",
                table: "MediaFiles",
                column: "UploadedByMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Members_UploadedByMemberId",
                table: "MediaFiles",
                column: "UploadedByMemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Members_UploadedByMemberId",
                table: "MediaFiles");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_UploadedByMemberId",
                table: "MediaFiles");
        }
    }
}
