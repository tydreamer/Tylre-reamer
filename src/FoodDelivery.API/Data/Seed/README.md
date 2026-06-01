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

## Restaurants and meals

| File | Contents |
|------|----------|
| `restaurants.csv` | 29 restaurants with image URLs and owner assignment |
| `meals.csv` | 4 meals per restaurant; `ImageUrl` optional; `MealType` optional (inferred when empty) |
| `MealTypes` table | Breakfast, Lunch, Dinner, Appetizers, Dessert (seeded from `MealTypeNames` constants) |

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
