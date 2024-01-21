using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    public partial class arrumarelresponsaveis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Responsavel_email",
                table: "Ativos");

            migrationBuilder.CreateTable(
                name: "ListResponsaveisAtivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email_responsavel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ativo_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListResponsaveisAtivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListResponsaveisAtivos_Ativos_Ativo_id",
                        column: x => x.Ativo_id,
                        principalTable: "Ativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListResponsaveisAtivos_Ativo_id",
                table: "ListResponsaveisAtivos",
                column: "Ativo_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListResponsaveisAtivos");

            migrationBuilder.AddColumn<string>(
                name: "Responsavel_email",
                table: "Ativos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
