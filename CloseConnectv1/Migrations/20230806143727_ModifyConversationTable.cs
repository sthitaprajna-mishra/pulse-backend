using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloseConnectv1.Migrations
{
    /// <inheritdoc />
    public partial class ModifyConversationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParticipantId",
                table: "Conversations",
                newName: "ParticipantTwoId");

            migrationBuilder.AddColumn<string>(
                name: "ParticipantOneId",
                table: "Conversations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipantOneId",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "ParticipantTwoId",
                table: "Conversations",
                newName: "ParticipantId");
        }
    }
}
