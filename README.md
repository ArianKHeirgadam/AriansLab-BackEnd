# AriansLab API

ASP.NET Core 8 API for AriansLab. Browser authentication uses short-lived JWT access tokens and rotating refresh tokens in `HttpOnly`, `Secure` cookies. Unsafe requests are protected by antiforgery tokens.

## Required configuration

Do not put production secrets in `appsettings*.json` or commit them to Git. Supply these values with environment variables or a managed secret store:

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__SecretKey
Cors__AllowedOrigins__0
AllowedHosts
```

`Jwt__SecretKey` must be a cryptographically random value of at least 64 characters. Access-token lifetime must be 5–30 minutes and refresh-token lifetime 1–30 days. See `AriansLab/appsettings.Example.json` for the complete shape.

For local development in PowerShell, set secrets only for the current terminal session:

```powershell
$env:ConnectionStrings__DefaultConnection = "YOUR_LOCAL_CONNECTION_STRING"
$env:Jwt__SecretKey = "YOUR_RANDOM_SECRET_WITH_AT_LEAST_64_CHARACTERS"
$env:Cors__AllowedOrigins__0 = "http://localhost:3000"
dotnet run --project AriansLab
```

Production must use HTTPS. Keep `AuthCookies__Secure=true`. `SameSite=Lax` is appropriate when the frontend and API are same-site. If they are hosted on different sites, use `SameSite=None`, keep Secure enabled, and configure one exact CORS origin per frontend.

## Database migration

Apply migrations before starting the updated API:

```bash
dotnet ef database update --project Persistence --startup-project AriansLab
```

The security migration revokes legacy refresh tokens because older records were stored in a different format. Existing users must sign in again once. Deploy the cookie-auth frontend and backend together during the same release window.

## Initial administrator

The administrator seeder is disabled by default. For the first deployment only, supply `AdminSeed__Enabled=true`, `AdminSeed__FullName`, `AdminSeed__Email`, `AdminSeed__UserName`, and a unique `AdminSeed__Password` of at least 12 characters. After the first successful start, remove those secret values and set `AdminSeed__Enabled=false`.

The seeder never overwrites an existing administrator password.

## Browser authentication flow

1. Call `GET /api/Auth/csrf-token` with credentials enabled.
2. Send the returned token in `X-CSRF-TOKEN` for every `POST`, `PUT`, `PATCH`, or `DELETE` request.
3. Call register or login. The API writes access and refresh cookies; tokens are not returned in JSON.
4. Fetch a new CSRF token after login or registration because the authenticated identity changed.
5. Use `GET /api/Auth/me` as the authoritative session check.
6. Call `POST /api/Auth/refresh-token` to rotate the refresh token and `POST /api/Auth/logout` to revoke it.

## Build and tests

```bash
dotnet restore AriansLab.sln
dotnet build AriansLab.sln --no-restore --configuration Release
dotnet test tests/AriansLab.ApiTests/AriansLab.ApiTests.csproj --no-build --configuration Release
```

If a local VSTest installation is unavailable, the same scenarios have a direct runner:

```bash
dotnet run --project tests/AriansLab.ApiTests/AriansLab.ApiTests.csproj --configuration Release
```

Tests cover password hashing and legacy upgrades, malformed hash rejection, mandatory CSRF, the complete register → cookie → `/me` → refresh → logout flow, customer denial on admin endpoints, immediate access-cookie invalidation after account or password changes, and refresh-token replay detection with session-family revocation.

## Production security checklist

- Rotate every database password, JWT key, administrator credential, and repository token that has ever been exposed.
- Restrict `AllowedHosts` and CORS to exact production hosts; never use wildcards with credentials.
- Terminate TLS only at a trusted reverse proxy and configure its address as a known proxy before accepting forwarded headers.
- Persist ASP.NET Core Data Protection keys in a protected shared key store when running multiple instances.
- Keep Swagger disabled in production, apply database backups, centralize logs, and alert on repeated 401/403/429 responses.
- Run dependency and secret scanning in CI and review Dependabot updates promptly.
- Review retention, consent, privacy, and breach-response requirements with qualified legal counsel for the deployment jurisdiction.
