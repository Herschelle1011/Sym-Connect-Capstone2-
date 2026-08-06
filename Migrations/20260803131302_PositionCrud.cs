using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class PositionCrud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ElectionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_Positions_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "ElectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_PositionId",
                table: "Candidates",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ElectionId",
                table: "Positions",
                column: "ElectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Positions_PositionId",
                table: "Candidates",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Positions_PositionId",
                table: "Candidates");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_PositionId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Candidates");
        }
    }
}
