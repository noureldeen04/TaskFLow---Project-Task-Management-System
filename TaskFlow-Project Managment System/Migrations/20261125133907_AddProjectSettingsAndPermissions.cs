using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow_Project_Managment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSettingsAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectPermissionSettings",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ViewAccess = table.Column<int>(type: "int", nullable: false),
                    EditAccess = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPermissionSettings", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_ProjectPermissionSettings_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectPermissionSettings");
        }
    }
}
