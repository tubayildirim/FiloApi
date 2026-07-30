using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Filo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRBACRoleAndManagerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Person",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Person",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Staff");

            migrationBuilder.CreateIndex(
                name: "IX_Person_ManagerId",
                table: "Person",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Person_ManagerId",
                table: "Person",
                column: "ManagerId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Person_ManagerId",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_ManagerId",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Person");
        }
    }
}
