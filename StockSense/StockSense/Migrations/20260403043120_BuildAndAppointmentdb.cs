using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockSense.Migrations
{
    /// <inheritdoc />
    public partial class BuildAndAppointmentdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoreServiceId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PreBuildPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompatibleBrand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompatibleModel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetCC = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreBuildPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreBuildPackageProduct",
                columns: table => new
                {
                    IncludedProductsId = table.Column<int>(type: "int", nullable: false),
                    PreBuildPackagesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreBuildPackageProduct", x => new { x.IncludedProductsId, x.PreBuildPackagesId });
                    table.ForeignKey(
                        name: "FK_PreBuildPackageProduct_PreBuildPackages_PreBuildPackagesId",
                        column: x => x.PreBuildPackagesId,
                        principalTable: "PreBuildPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreBuildPackageProduct_Products_IncludedProductsId",
                        column: x => x.IncludedProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreServiceId",
                table: "Products",
                column: "StoreServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PreBuildPackageProduct_PreBuildPackagesId",
                table: "PreBuildPackageProduct",
                column: "PreBuildPackagesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_StoreServices_StoreServiceId",
                table: "Products",
                column: "StoreServiceId",
                principalTable: "StoreServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_StoreServices_StoreServiceId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "PreBuildPackageProduct");

            migrationBuilder.DropTable(
                name: "PreBuildPackages");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreServiceId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreServiceId",
                table: "Products");
        }
    }
}
