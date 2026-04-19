# Car Lookup

ASP.NET Core 9 Razor Pages app that browses U.S. vehicle makes, years, and models via the [NHTSA vPIC API](https://vpic.nhtsa.dot.gov/api/).

Pick a make → year (1995–present) → vehicle type, and the Results page lists every matching model.

## Project layout

```
src/
  CarLookup.Core/   # domain models, DTOs, typed NHTSA client, catalog service, caching
  CarLookup.Web/    # Razor Pages host (Index + Results), wwwroot static assets
tests/
  CarLookup.Tests/  # xUnit + Moq + FluentAssertions
Dockerfile          # multi-stage build targeting linux/amd64
```

---

## Run with Docker (preferred)

### Prerequisites
- Docker Desktop or any recent Docker Engine (≥ 20.10)

### Build

```bash
docker build -t carlookup:local .
```

### Run

```bash
docker run --rm -p 8080:8080 --name carlookup carlookup:local
```

Open **http://localhost:8080**.

Stop with `Ctrl+C` (or `docker stop carlookup` from another shell).

### Override settings at runtime

ASP.NET Core maps `__` in env var names to nested JSON keys, so you can override any `appsettings.json` value without rebuilding:

```bash
docker run --rm -p 8080:8080 \
  -e Nhtsa__TimeoutSeconds=30 \
  -e Nhtsa__MakesCacheMinutes=60 \
  carlookup:local
```

---

## Run without Docker

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

Verify with:

```bash
dotnet --version   # should print 9.x
```

### Clone, restore, build, run

```bash
git clone <repo-url> carlookup
cd carlookup

dotnet restore CarLookup.sln
dotnet build   CarLookup.sln -c Release

dotnet run --project src/CarLookup.Web/CarLookup.Web.csproj
```

The console prints the listen URL (typically `http://localhost:5000` and `https://localhost:5001` in dev). Open that in a browser.

---

## Run the tests

```bash
dotnet test tests/CarLookup.Tests/CarLookup.Tests.csproj
```

You should see **Passed: 12**.

---

## Configuration

All runtime settings live in `src/CarLookup.Web/appsettings.json`. The section that matters is `Nhtsa`:

| Key | Default | Purpose |
|---|---|---|
| `Nhtsa:BaseUrl` | `https://vpic.nhtsa.dot.gov/api/vehicles/` | Root of the NHTSA vPIC API. Must end with a trailing slash. Change this to point at a mock or proxy. |
| `Nhtsa:TimeoutSeconds` | `15` | Per-request HTTP timeout against NHTSA. |
| `Nhtsa:MakesCacheMinutes` | `1440` (24 h) | How long the full Makes list is cached in memory. Makes rarely change — a long TTL is fine. |
| `Nhtsa:VehicleTypesCacheMinutes` | `60` | How long per-make Vehicle Types are cached. |
| `Nhtsa:EarliestYear` | `1995` | Oldest model year shown in the Year dropdown. Latest year is always the current year. |

### Ways to override

1. **Edit `appsettings.json`** for a permanent local change.
2. **Create `appsettings.Development.json`** (or `appsettings.Production.json`) — values there override the base file in that environment.
3. **Environment variables** using `__` as the separator: `Nhtsa__BaseUrl=https://mock.example/`, `Nhtsa__TimeoutSeconds=30`, etc. These override everything else and are the recommended way to configure the Docker image.

Logging verbosity lives under `Logging:LogLevel` in the same file — bump `CarLookup` to `Debug` to see cache hits/misses and NHTSA request paths.
