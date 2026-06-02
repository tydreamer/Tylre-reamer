# Database seed data

CSV files in this folder are loaded on API startup when the database is empty (see `DbSeeder.cs`).

## Default password (all seeded accounts)

**`Password123!`**

Change it in `DbSeeder.cs` (`DefaultSeedPassword`) if you want a different demo password.

## Seeded accounts (`users.csv`)

| Role | Name | Email |
|------|------|--------|
| Admin | Administrator | `admin@example.com` |
| Customer | Demo Customer | `customer@example.com` |

## Restaurant owners (`restaurants.csv`)

Each restaurant row includes `OwnerName` and `OwnerEmail`. One owner account is created per restaurant (29 owners). Emails follow `owner.<slug>@example.com` (see the CSV for exact addresses).

Example: Bella Napoli → `owner.bella-napoli@example.com` (Marco Rossi).

## Cuisines (`Cuisines` table)

Seeded from `CuisineNames` in `FoodDelivery.API/Constants/CuisineNames.cs` before restaurants are loaded:

| Name |
|------|
| Italian |
| French |
| Chinese |
| Japanese |
| Mexican |

## Meal types (`MealTypes` table)

Seeded from `MealTypeNames` in `FoodDelivery.API/Constants/MealTypeNames.cs` before meals are loaded:

| Name | Typical use |
|------|-------------|
| Breakfast | Morning items (pancakes, coffee, omelettes) |
| Lunch | Mid-day mains (burgers, bowls, sandwiches, tacos) |
| Dinner | Evening mains (pasta, curry, grilled plates, sushi rolls) |
| Appetizers | Starters and sides (soup, wings, dumplings, fries) |
| Dessert | Sweets (cake, ice cream, pastries) |

`GET /api/meal-types` returns these rows for the owner meal form.

## Restaurants and meals

| File | Contents |
|------|----------|
| `restaurants.csv` | 29 restaurants with image URLs, owner assignment, cuisine, and coordinates |
| `meals.csv` | 4 meals per restaurant (see columns below) |

### `restaurants.csv` columns

| Column | Required | Description |
|--------|----------|-------------|
| `Name` | Yes | Restaurant name |
| `Description` | Yes | Short description |
| `ImageUrl` | Yes | Card image URL |
| `OwnerName` | Yes | Owner display name (account created if missing) |
| `OwnerEmail` | Yes | Owner login email |
| `Cuisine` | Yes | One of: `Italian`, `French`, `Chinese`, `Japanese`, `Mexican` |
| `Latitude` | Yes | Decimal degrees (e.g. `40.7128`) |
| `Longitude` | Yes | Decimal degrees (e.g. `-74.0060`) |

### `meals.csv` columns

| Column | Required | Description |
|--------|----------|-------------|
| `RestaurantName` | Yes | Must match a name in `restaurants.csv` |
| `Name` | Yes | Meal name |
| `Description` | Yes | Short description |
| `Price` | Yes | Decimal price (e.g. `12.50`) |
| `ImageUrl` | No | Leave empty to auto-generate via `MealImageUrlBuilder` on seed |
| `MealType` | Yes* | One of: `Breakfast`, `Lunch`, `Dinner`, `Appetizers`, `Dessert` |

\*If `MealType` is blank, `DbSeeder` infers a type from the restaurant and meal name (fallback only).

Example row:

```csv
Bella Napoli,Tiramisu,Espresso-soaked ladyfingers and mascarpone,6.50,,Dessert
```

## Fresh seed (empty database)

1. Create PostgreSQL database and set `ConnectionStrings:DefaultConnection` in `appsettings.Development.json`.
2. Run migrations: `dotnet ef database update` (from `src/FoodDelivery.API`).
3. Start the API — seed runs automatically in Development when tables are empty.

To re-seed from scratch, drop the database (or delete all rows) and restart the API.

## What is not seeded

- Orders, coupons, or cart data
- Google-linked accounts (`GoogleSubjectId` remains null for CSV users)

## Legacy note

Older databases may still have a single `owner@example.com` owning every restaurant. Re-seed or migrate manually if you need per-restaurant owners.
