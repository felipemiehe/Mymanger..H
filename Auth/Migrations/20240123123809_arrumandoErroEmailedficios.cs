using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    public partial class arrumandoErroEmailedficios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email_responsavel",
                table: "ListResponsaveisAtivos",
                newName: "email_responsavel_criado");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email_responsavel_criado",
                table: "ListResponsaveisAtivos",
                newName: "email_responsavel");
        }
    }
}
