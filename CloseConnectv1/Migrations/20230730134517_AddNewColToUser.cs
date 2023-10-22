﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloseConnectv1.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundPictureURL",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundPictureURL",
                table: "AspNetUsers");
        }
    }
}
