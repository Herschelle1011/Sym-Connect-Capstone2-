using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class EventFacilityModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Facilities_FacilityId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_FacilityId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                table: "Events");

            migrationBuilder.CreateTable(
                name: "EventFacility",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId1 = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventFacility", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_EventFacility_Events_EventId1",
                        column: x => x.EventId1,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventFacility_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "FacilityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventFacility_EventId1",
                table: "EventFacility",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_EventFacility_FacilityId",
                table: "EventFacility",
                column: "FacilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventFacility");

            migrationBuilder.AddColumn<int>(
                name: "FacilityId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_FacilityId",
                table: "Events",
                column: "FacilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Facilities_FacilityId",
                table: "Events",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId");
        }
    }
}
