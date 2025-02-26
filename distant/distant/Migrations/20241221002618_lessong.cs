﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace distant.Migrations
{
    /// <inheritdoc />
    public partial class lessong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "VerificationCode" },
                values: new object[] { 1, "VLSU12" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
