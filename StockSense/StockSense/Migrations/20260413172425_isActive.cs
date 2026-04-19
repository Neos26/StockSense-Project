using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockSense.Migrations
{
    /// <inheritdoc />
    public partial class isActive : Migration
    {
        /// <inheritdoc />


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PreBuildPackages");
        }
    }
}
