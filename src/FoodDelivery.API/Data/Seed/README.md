# Seed data

CSV files here are loaded on API startup in Development when the tables are empty (see `DbSeeder.cs`).

## Seeded accounts

Passwords are stored as plain text in the CSVs and BCrypt-hashed by the seeder at runtime.

### `users.csv` — admin and customer

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@gmail.com` | `adminpass` |
| Customer | `customer@gmail.com` | `customerpass` |

### Owner accounts — generated from `restaurants.csv`

Owners are not listed in `users.csv`. The seeder groups the 25 restaurants into owners of 4–7 each (fixed RNG seed 42 for stability) and creates `owner1@gmail.com` … `ownerN@gmail.com`, all with password `ownerpass`.

## `restaurants.csv` columns

| Column | Description |
|--------|-------------|
| `Name` | Restaurant name |
| `Description` | Short description |
| `ImageUrl` | Card image URL |
| `Cuisine` | One of: `Italian`, `French`, `Chinese`, `Japanese`, `Mexican` |
| `Latitude` | Decimal degrees |
| `Longitude` | Decimal degrees |

Owner columns were removed — ownership is assigned dynamically by `DbSeeder`.

## `meals.csv` columns

| Column | Required | Description |
|--------|----------|-------------|
| `RestaurantName` | Yes | Must match a name in `restaurants.csv` |
| `Name` | Yes | Meal name |
| `Description` | Yes | Short description |
| `Price` | Yes | Decimal (e.g. `12.50`) |
| `ImageUrl` | No | Leave empty to auto-generate |
| `MealType` | Yes* | `Breakfast`, `Lunch`, `Dinner`, `Appetizers`, or `Dessert` |

\* If blank, the seeder infers a type from the meal/restaurant name as a fallback.

## What is not seeded

- Orders, coupons, or cart data
- Google-linked accounts (`GoogleSubjectId` is null for all CSV users)
