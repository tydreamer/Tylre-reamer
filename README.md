# Food Delivery

Blazor WebAssembly front-end + ASP.NET Core API back-end.

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 or later |
| [PostgreSQL](https://www.postgresql.org/download/) | 14 or later |

## Quick start

### 1. Configure the database

Open `src/FoodDelivery.API/appsettings.Development.json` and set your PostgreSQL credentials:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=fooddelivery;Username=postgres;Password=password"
}
```

The default values (`postgres` / `password`) match a stock local PostgreSQL install. Change them if your setup differs.

### 2. Run the API

```bash
cd src/FoodDelivery.API
dotnet run
```

The API starts on **http://localhost:5115**.

On first start it automatically:
- Runs all EF Core migrations (creates the database schema)
- Seeds users, restaurants, and meals

### 3. Run the front-end

Open a second terminal:

```bash
cd src/FoodDelivery.Web
dotnet run
```

The app opens at **http://localhost:5196**.

---

## Seed accounts

All accounts are created automatically on first API start.

### Admin

| Field | Value |
|-------|-------|
| Email | `admin@gmail.com` |
| Password | `adminpass` |

### Customer

| Field | Value |
|-------|-------|
| Email | `customer@gmail.com` |
| Password | `customerpass` |

### Owners

25 restaurants are seeded and grouped into owners (4–7 restaurants each).

| Email | Password |
|-------|----------|
| `owner1@gmail.com` | `ownerpass` |
| `owner2@gmail.com` | `ownerpass` |
| … | `ownerpass` |
| `ownerN@gmail.com` | `ownerpass` |

Sign in as any owner to manage their assigned restaurants and orders.

---

## Re-seeding from scratch

Drop and recreate the database, then restart the API:

```bash
# psql
DROP DATABASE fooddelivery;
CREATE DATABASE fooddelivery;
```

```bash
cd src/FoodDelivery.API
dotnet run
```

---

## Optional: Google OAuth

Already wired up for local development via credentials in `appsettings.Development.json`. No extra steps needed to use the "Sign in with Google" button locally.

To disable it, remove or clear the `Google:ClientId` and `Google:ClientSecret` values — the app falls back to email/password only.

---

## Configuration reference (`appsettings.json`)

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:Key` | Secret key for JWT signing (min 32 chars) |
| `Cors:AllowedOrigins` | Comma-separated list of allowed front-end origins |
| `App:WebBaseUrl` | Front-end base URL (used for OAuth redirects) |
| `Google:ClientId` | Google OAuth client ID (optional) |
| `Google:ClientSecret` | Google OAuth client secret (optional) |
| `Uploads:RootPath` | Folder where uploaded images are stored |
