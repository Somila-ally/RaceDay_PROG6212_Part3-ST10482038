using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RaceDay.Api.Data;

#nullable disable

namespace RaceDay.Api.Migrations;

[DbContext(typeof(RaceDayDbContext))]
[Migration("20261109090000_AddImageUrls")]
public partial class AddImageUrls : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "BannerImageUrl", table: "Events", type: "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ProfileImageUrl", table: "Users", type: "nvarchar(500)", maxLength: 500, nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BannerImageUrl", table: "Events");
        migrationBuilder.DropColumn(name: "ProfileImageUrl", table: "Users");
    }
}
