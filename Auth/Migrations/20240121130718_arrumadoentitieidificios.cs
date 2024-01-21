using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    public partial class arrumadoentitieidificios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Responsavel_email",
                table: "AtivoxUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Ativos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroAptos",
                table: "Ativos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsavel_email",
                table: "Ativos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Responsavel_email",
                table: "AtivoxUsers");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Ativos");

            migrationBuilder.DropColumn(
                name: "NumeroAptos",
                table: "Ativos");

            migrationBuilder.DropColumn(
                name: "Responsavel_email",
                table: "Ativos");
        }
    }
}
