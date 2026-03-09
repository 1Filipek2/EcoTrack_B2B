using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoTrack.Infrastructure.Migrations;

public partial class AddEmissionCategoryEmbedding : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "Embedding",
            table: "EmissionCategories",
            type: "vector(1536)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Embedding",
            table: "EmissionCategories");
    }
}

