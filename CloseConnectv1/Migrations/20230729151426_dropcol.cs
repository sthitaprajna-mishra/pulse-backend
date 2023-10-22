using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloseConnectv1.Migrations
{
    /// <inheritdoc />
    public partial class dropcol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AncestorCommentId",
                table: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AncestorCommentId",
                table: "Comments",
                type: "int",
                nullable: true);
        }
    }
}
