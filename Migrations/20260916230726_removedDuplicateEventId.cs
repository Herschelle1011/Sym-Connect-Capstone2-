using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    public partial class removedDuplicateEventId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================
            // 1. CREATE A TEMPORARY TABLE
            // =====================================================
            // We save the existing EventId + FacilityId
            // relationships before rebuilding EventFacilities.

            migrationBuilder.CreateTable(
                name: "EventFacilities_Temp",
                columns: table => new
                {
                    EventId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    FacilityId = table.Column<int>(
                        type: "int",
                        nullable: false)
                });


            // =====================================================
            // 2. COPY EXISTING RELATIONSHIPS
            // =====================================================

            migrationBuilder.Sql(@"
                INSERT INTO EventFacilities_Temp
                    (EventId, FacilityId)

                SELECT EventId1, FacilityId
                FROM EventFacilities;
            ");


            // =====================================================
            // 3. DROP OLD TABLE
            // =====================================================
            // This removes:
            //
            // EventId  (old Identity)
            // EventId1 (duplicate FK)
            // FacilityId

            migrationBuilder.DropTable(
                name: "EventFacilities");


            // =====================================================
            // 4. CREATE THE CORRECT TABLE
            // =====================================================

            migrationBuilder.CreateTable(
                name: "EventFacilities",
                columns: table => new
                {
                    EventId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    FacilityId = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    // Composite Primary Key
                    table.PrimaryKey(
                        name: "PK_EventFacilities",
                        columns: x => new
                        {
                            x.EventId,
                            x.FacilityId
                        });


                    // Event relationship
                    table.ForeignKey(
                        name: "FK_EventFacilities_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);


                    // Facility relationship
                    table.ForeignKey(
                        name: "FK_EventFacilities_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "FacilityId",
                        onDelete: ReferentialAction.Restrict);
                });


            // =====================================================
            // 5. INDEX FOR FACILITY
            // =====================================================

            migrationBuilder.CreateIndex(
                name: "IX_EventFacilities_FacilityId",
                table: "EventFacilities",
                column: "FacilityId");


            // =====================================================
            // 6. RESTORE EXISTING DATA
            // =====================================================

            migrationBuilder.Sql(@"
                INSERT INTO EventFacilities
                    (EventId, FacilityId)

                SELECT EventId, FacilityId
                FROM EventFacilities_Temp;
            ");


            // =====================================================
            // 7. REMOVE TEMPORARY TABLE
            // =====================================================

            migrationBuilder.DropTable(
                name: "EventFacilities_Temp");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // For your development database, we don't want to
            // accidentally recreate the incorrect EventId1 design.
            //
            // If you need to roll this migration back later,
            // create a proper rollback migration instead.

            throw new NotSupportedException(
                "Rolling back this migration would recreate the old incorrect EventFacility relationship.");
        }
    }
}