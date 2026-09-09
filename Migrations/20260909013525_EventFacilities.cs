using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class EventFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFacility_Events_EventId1",
                table: "EventFacility");

            migrationBuilder.DropForeignKey(
                name: "FK_EventFacility_Facilities_FacilityId",
                table: "EventFacility");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventFacility",
                table: "EventFacility");

            migrationBuilder.RenameTable(
                name: "EventFacility",
                newName: "EventFacilities");

            migrationBuilder.RenameIndex(
                name: "IX_EventFacility_FacilityId",
                table: "EventFacilities",
                newName: "IX_EventFacilities_FacilityId");

            migrationBuilder.RenameIndex(
                name: "IX_EventFacility_EventId1",
                table: "EventFacilities",
                newName: "IX_EventFacilities_EventId1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventFacilities",
                table: "EventFacilities",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventFacilities_Events_EventId1",
                table: "EventFacilities",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventFacilities_Facilities_FacilityId",
                table: "EventFacilities",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFacilities_Events_EventId1",
                table: "EventFacilities");

            migrationBuilder.DropForeignKey(
                name: "FK_EventFacilities_Facilities_FacilityId",
                table: "EventFacilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventFacilities",
                table: "EventFacilities");

            migrationBuilder.RenameTable(
                name: "EventFacilities",
                newName: "EventFacility");

            migrationBuilder.RenameIndex(
                name: "IX_EventFacilities_FacilityId",
                table: "EventFacility",
                newName: "IX_EventFacility_FacilityId");

            migrationBuilder.RenameIndex(
                name: "IX_EventFacilities_EventId1",
                table: "EventFacility",
                newName: "IX_EventFacility_EventId1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventFacility",
                table: "EventFacility",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventFacility_Events_EventId1",
                table: "EventFacility",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventFacility_Facilities_FacilityId",
                table: "EventFacility",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
