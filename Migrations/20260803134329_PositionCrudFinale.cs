using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class PositionCrudFinale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Positions_PositionId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Elections_ElectionId",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_ElectionId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_PositionId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ElectionId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Candidates");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "positions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_positions",
                table: "positions",
                column: "PositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_positions",
                table: "positions");

            migrationBuilder.RenameTable(
                name: "positions",
                newName: "Positions");

            migrationBuilder.AddColumn<int>(
                name: "ElectionId",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ElectionId",
                table: "Positions",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_PositionId",
                table: "Candidates",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Positions_PositionId",
                table: "Candidates",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Elections_ElectionId",
                table: "Positions",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
