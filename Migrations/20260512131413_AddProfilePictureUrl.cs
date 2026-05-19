using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthMembers_AspNetUsers_UserId",
                table: "YouthMembers");

            migrationBuilder.DropIndex(
                name: "IX_YouthMembers_UserId",
                table: "YouthMembers");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YouthMembers_UserId",
                table: "YouthMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthMembers_AspNetUsers_UserId",
                table: "YouthMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthMembers_AspNetUsers_UserId",
                table: "YouthMembers");

            migrationBuilder.DropIndex(
                name: "IX_YouthMembers_UserId",
                table: "YouthMembers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_YouthMembers_UserId",
                table: "YouthMembers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthMembers_AspNetUsers_UserId",
                table: "YouthMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
