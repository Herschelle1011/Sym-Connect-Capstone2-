using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class addFacilityandEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Facilities_FacilityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupDetails_Members_MemberId",
                table: "GroupDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Members_LeaderId",
                table: "Groups");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "GroupDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Facilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Facilities_FacilityId",
                table: "Activities",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupDetails_Members_MemberId",
                table: "GroupDetails",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Members_LeaderId",
                table: "Groups",
                column: "LeaderId",
                principalTable: "Members",
                principalColumn: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Facilities_FacilityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupDetails_Members_MemberId",
                table: "GroupDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Members_LeaderId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Facilities");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "GroupDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Facilities_FacilityId",
                table: "Activities",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupDetails_Members_MemberId",
                table: "GroupDetails",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Members_LeaderId",
                table: "Groups",
                column: "LeaderId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
