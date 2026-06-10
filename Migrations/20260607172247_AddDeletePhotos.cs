using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletePhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacilityPhoto_Facilities_FacilityId",
                table: "FacilityPhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FacilityPhoto",
                table: "FacilityPhoto");

            migrationBuilder.RenameTable(
                name: "FacilityPhoto",
                newName: "FacilityPhotos");

            migrationBuilder.RenameIndex(
                name: "IX_FacilityPhoto_FacilityId",
                table: "FacilityPhotos",
                newName: "IX_FacilityPhotos_FacilityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FacilityPhotos",
                table: "FacilityPhotos",
                column: "FacilityPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacilityPhotos_Facilities_FacilityId",
                table: "FacilityPhotos",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacilityPhotos_Facilities_FacilityId",
                table: "FacilityPhotos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FacilityPhotos",
                table: "FacilityPhotos");

            migrationBuilder.RenameTable(
                name: "FacilityPhotos",
                newName: "FacilityPhoto");

            migrationBuilder.RenameIndex(
                name: "IX_FacilityPhotos_FacilityId",
                table: "FacilityPhoto",
                newName: "IX_FacilityPhoto_FacilityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FacilityPhoto",
                table: "FacilityPhoto",
                column: "FacilityPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacilityPhoto_Facilities_FacilityId",
                table: "FacilityPhoto",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
