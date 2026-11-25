using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow_Project_Managment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Template",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Template",
                table: "Projects");
        }
    }
}
