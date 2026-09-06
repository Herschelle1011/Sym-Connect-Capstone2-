using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class newFkFolderMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MediaFolders_CreatedByMemberId",
                table: "MediaFolders",
                column: "CreatedByMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFolders_Members_CreatedByMemberId",
                table: "MediaFolders",
                column: "CreatedByMemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFolders_Members_CreatedByMemberId",
                table: "MediaFolders");

            migrationBuilder.DropIndex(
                name: "IX_MediaFolders_CreatedByMemberId",
                table: "MediaFolders");
        }
    }
}
