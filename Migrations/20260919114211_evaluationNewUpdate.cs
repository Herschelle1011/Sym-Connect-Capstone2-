using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class evaluationNewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Evaluations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_CreatedByUserId",
                table: "Evaluations",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_AspNetUsers_CreatedByUserId",
                table: "Evaluations",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_AspNetUsers_CreatedByUserId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_CreatedByUserId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Evaluations");
        }
    }
}
