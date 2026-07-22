using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260722150000_AddProjectCommentModeration")]
public partial class AddProjectCommentModeration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsCustomerCommentApproved",
            table: "Projects",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsCustomerCommentApproved",
            table: "Projects");
    }
}
