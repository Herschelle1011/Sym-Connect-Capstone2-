using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class changesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_YouthGroups_YouthGroupId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthGroups_YouthMembers_LeaderYouthMemberId",
                table: "YouthGroups");

            migrationBuilder.DropTable(
                name: "YouthMembers");

            migrationBuilder.DropTable(
                name: "YouthGroups");

            migrationBuilder.RenameColumn(
                name: "YouthGroupId",
                table: "Activities",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Activities_YouthGroupId",
                table: "Activities",
                newName: "IX_Activities_GroupId");

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateRegistered = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeaderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_Groups_Members_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupDetails",
                columns: table => new
                {
                    GroupDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDetails", x => x.GroupDetailsId);
                    table.ForeignKey(
                        name: "FK_GroupDetails_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupDetails_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupDetails_GroupId",
                table: "GroupDetails",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupDetails_MemberId",
                table: "GroupDetails",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LeaderId",
                table: "Groups",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Groups_GroupId",
                table: "Activities",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Groups_GroupId",
                table: "Activities");

            migrationBuilder.DropTable(
                name: "GroupDetails");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "Activities",
                newName: "YouthGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Activities_GroupId",
                table: "Activities",
                newName: "IX_Activities_YouthGroupId");

            migrationBuilder.CreateTable(
                name: "YouthGroups",
                columns: table => new
                {
                    YouthGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaderYouthMemberId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouthGroups", x => x.YouthGroupId);
                });

            migrationBuilder.CreateTable(
                name: "YouthMembers",
                columns: table => new
                {
                    YouthMemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    YouthGroupId = table.Column<int>(type: "int", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouthMembers", x => x.YouthMemberId);
                    table.ForeignKey(
                        name: "FK_YouthMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YouthMembers_YouthGroups_YouthGroupId",
                        column: x => x.YouthGroupId,
                        principalTable: "YouthGroups",
                        principalColumn: "YouthGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YouthGroups_LeaderYouthMemberId",
                table: "YouthGroups",
                column: "LeaderYouthMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_YouthMembers_UserId",
                table: "YouthMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_YouthMembers_YouthGroupId",
                table: "YouthMembers",
                column: "YouthGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_YouthGroups_YouthGroupId",
                table: "Activities",
                column: "YouthGroupId",
                principalTable: "YouthGroups",
                principalColumn: "YouthGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthGroups_YouthMembers_LeaderYouthMemberId",
                table: "YouthGroups",
                column: "LeaderYouthMemberId",
                principalTable: "YouthMembers",
                principalColumn: "YouthMemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
