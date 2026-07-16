using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260715121500_RevokeLegacyRefreshTokens")]
public partial class RevokeLegacyRefreshTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Previous versions persisted raw refresh tokens. They cannot be safely
        // converted to one-way hashes, so revoke them during the coordinated deploy.
        migrationBuilder.Sql("DELETE FROM [RefreshTokens]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revoked credentials must never be restored.
    }
}
