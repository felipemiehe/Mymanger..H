using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    public partial class arrumaCodUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "Ativos",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoUnico",
                table: "Ativos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ativos_CodigoUnico",
                table: "Ativos",
                column: "CodigoUnico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ativos_Endereco",
                table: "Ativos",
                column: "Endereco",
                unique: true,
                filter: "[Endereco] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ativos_CodigoUnico",
                table: "Ativos");

            migrationBuilder.DropIndex(
                name: "IX_Ativos_Endereco",
                table: "Ativos");

            migrationBuilder.DropColumn(
                name: "CodigoUnico",
                table: "Ativos");

            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "Ativos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
