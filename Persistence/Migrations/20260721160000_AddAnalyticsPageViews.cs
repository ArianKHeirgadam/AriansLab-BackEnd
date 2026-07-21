using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260721160000_AddAnalyticsPageViews")]
public partial class AddAnalyticsPageViews : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PageViews",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                VisitorIdHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                SessionIdHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                ReferrerHost = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: true),
                DeviceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Browser = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PageViews", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PageViews_CreatedAt",
            table: "PageViews",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_PageViews_Path_CreatedAt",
            table: "PageViews",
            columns: new[] { "Path", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_PageViews_SessionIdHash_CreatedAt",
            table: "PageViews",
            columns: new[] { "SessionIdHash", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_PageViews_VisitorIdHash_CreatedAt",
            table: "PageViews",
            columns: new[] { "VisitorIdHash", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PageViews");
    }
}
