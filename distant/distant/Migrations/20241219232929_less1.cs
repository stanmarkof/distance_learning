using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace distant.Migrations
{
    /// <inheritdoc />
    public partial class less1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonMaterials_Lessons_LessonId",
                table: "LessonMaterials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonMaterials",
                table: "LessonMaterials");

            migrationBuilder.RenameTable(
                name: "LessonMaterials",
                newName: "LessonMaterial");

            migrationBuilder.RenameIndex(
                name: "IX_LessonMaterials_LessonId",
                table: "LessonMaterial",
                newName: "IX_LessonMaterial_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonMaterial",
                table: "LessonMaterial",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonMaterial_Lessons_LessonId",
                table: "LessonMaterial",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonMaterial_Lessons_LessonId",
                table: "LessonMaterial");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonMaterial",
                table: "LessonMaterial");

            migrationBuilder.RenameTable(
                name: "LessonMaterial",
                newName: "LessonMaterials");

            migrationBuilder.RenameIndex(
                name: "IX_LessonMaterial_LessonId",
                table: "LessonMaterials",
                newName: "IX_LessonMaterials_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonMaterials",
                table: "LessonMaterials",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonMaterials_Lessons_LessonId",
                table: "LessonMaterials",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
