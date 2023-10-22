using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloseConnectv1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "Conversations",
                newName: "LatestDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatestDate",
                table: "Conversations",
                newName: "CreateDate");
        }
    }
}
