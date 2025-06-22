using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SquaresApi.Migrations
{
    /// <inheritdoc />
    public partial class CoordinateConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Points_CoordinateX_CoordinateY",
                table: "Points",
                columns: new[] { "CoordinateX", "CoordinateY" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Points_CoordinateX_CoordinateY",
                table: "Points");
        }
    }
}
