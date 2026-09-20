# E-commerce API

A full-featured e-commerce backend built with **ASP.NET Core Web API (.NET 10)**, designed and built from scratch as a deep-dive learning project — with a strong focus on clean architecture, security, and correctness under real-world conditions (concurrency, transactions, race conditions).

Repository: [github.com/bulga30000-art/E-commerce](https://github.com/bulga30000-art/E-commerce)

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Core Features by Domain](#core-features-by-domain)
- [Authentication & Authorization](#authentication--authorization)
- [Order Status Workflow](#order-status-workflow)
- [Concurrency & Data Integrity](#concurrency--data-integrity)
- [Error Handling](#error-handling)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Design Decisions (FAQ)](#design-decisions-faq)

---

## Overview

This project is a complete backend for an e-commerce platform, covering the full lifecycle of a customer's journey: registration and login, browsing products, placing orders, paying for them, tracking order status, and earning/redeeming loyalty points — plus an admin side for managing the catalog, orders, and payments.

It was built in six planned phases:

| Phase | Scope |
|---|---|
| 0 — Foundation | Repository pattern, Unit of Work, custom exceptions, global exception middleware, pagination |
| 1 — Identity | ASP.NET Core Identity, JWT authentication, role seeding (Customer/Admin) |
| 2 — Basic CRUD | Categories, Products (with image upload), Shippers, Order Statuses, Customers |
| 3 — Checkout | Order placement with stock validation, price snapshotting, atomic transactions |
| 4 — Order Status Workflow | State machine for order status transitions, cancellation with restock |
| 5 — Payments | Credit Card (instant) and Cash on Delivery (confirmed on delivery) |
| 6 — Loyalty Points | Points earned on delivery, redeemable at checkout |

---

## Tech Stack

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** (Database-First scaffold from an existing SQL Server database, `store_1`)
- **SQL Server**
- **ASP.NET Core Identity** + **JWT Bearer Authentication**
- **Swashbuckle (Swagger)** for interactive API documentation
- **User Secrets** for local secret management (JWT signing key)

---

## Architecture

The project follows a strict layered architecture with a one-way dependency flow:

```
Controller  →  Service  →  Repository (per entity)  →  Unit of Work  →  DbContext
```

**Why this shape, specifically:**

- **Thin Controllers.** Controllers only extract data from the request (route params, body, JWT claims) and call a Service method. No business logic lives in a controller.
- **No Generic Repository.** Each entity (`Product`, `Order`, `Customer`, ...) has its own dedicated repository interface and implementation. This keeps each repository's surface area meaningful to that entity, instead of forcing every entity through one generic `IRepository<T>` that doesn't fit all of them equally well.
- **Unit of Work.** A single `IUnitOfWork` aggregates every repository and exposes one `SaveChangesAsync()`, so a single business operation that touches multiple entities (e.g., checkout touching `Product`, `Order`, and `Customer`) commits as one unit. It also owns transaction boundaries (`BeginTransactionAsync` / `CommitTransactionAsync` / `RollbackTransactionAsync`) without leaking EF Core types (like `IDbContextTransaction`) into the Service layer.
- **Service Layer owns business rules.** Anything beyond "is this field the right shape" (e.g., "does this `CategoryId` actually exist?", "is there enough stock?", "can this order transition from `Pending` to `Shipped`?") lives in a Service, not in a DTO or a Controller.
- **Purpose-specific DTOs.** No EF Core entity is ever returned directly from an endpoint. Every endpoint has its own `*CreateDto` / `*UpdateDto` / `*ReadDto` shaped exactly for that use case.

---

## Project Structure

```
E-commerce/
├── Controllers/         # Thin HTTP layer — one controller per resource
├── Service/             # Business logic (one service per domain)
├── Repositories/        # One repository per entity + UnitOfWork
├── DTOs/                # Request/response contracts, grouped by domain
├── Models/              # EF Core entities (database-first scaffolded)
├── Data/                # StoreContext (DbContext)
├── Identity/             # ApplicationUser (extends IdentityUser)
├── Exceptions/           # AppException hierarchy (NotFound/BadRequest/Conflict)
├── Middleware/            # Global exception handling middleware
├── Validation/            # Custom validation attributes (file upload rules)
├── Common/                # Cross-cutting constants (OrderStatusIds, LoyaltyConstants, PaymentStatuses...)
├── FileSettings/          # Upload configuration (allowed extensions, size limits)
└── wwwroot/images/products/  # Disk-based product image storage
```

---

## Core Features by Domain

### Products
- Full CRUD (Admin-only writes, public reads)
- Image upload with server-side validation: allowed extensions and max file size, enforced via custom `ValidationAttribute`s (`AllowedExtensionsAttribute`, `MaxFileSizeAttribute`)
- Uploaded images are stored on disk under `wwwroot/images/products/`, saved with a **generated GUID filename** (never the original filename) to prevent filename collisions and path-based attacks
- Pagination, filtering, and sorting on list endpoints, implemented via `IQueryable` projection (`.Select()`) rather than `.Include()`, avoiding unnecessary joins

### Categories, Shippers, Order Statuses, Customers
- Standard CRUD, Admin-only writes, open reads (except Customers, which is fully Admin-only)
- Same pagination/filtering/sorting pattern as Products

### Checkout (Orders)
- A customer submits a list of `{ productId, quantity }` items
- Server-side **price snapshotting**: unit prices are read from the database at checkout time and stored on the order line — a later price change never affects an existing order
- Stock is validated and decremented **atomically** (see [Concurrency](#concurrency--data-integrity))
- Optional loyalty points redemption, capped at the order's total value
- `CustomerId` is taken exclusively from the JWT claims — never from the request body — preventing a customer from placing an order on someone else's behalf

### Payments
- Nested resource route: `POST /api/orders/{orderId}/payment`
- **Credit Card**: confirmed immediately (simulated)
- **Cash on Delivery**: stays `Pending` until the order actually reaches `Delivered` status, at which point it's automatically marked `Completed` as part of the same transaction that updates the order status — no separate manual step required in the common case
- A manual status update endpoint remains available for the `Failed` case (e.g., customer refuses to pay on delivery)

### Loyalty Points
- Points are earned **only** when an order reaches `Delivered` — not at checkout, not at `Shipped`
- Points can be redeemed at checkout, converting redeemed points into a discount off the order total
- If an order with redeemed points is later cancelled, the points are refunded to the customer

---

## Authentication & Authorization

- **ASP.NET Core Identity** manages `AspNetUsers` / `AspNetRoles`, extended with a linked `Customer` record (business-domain data, separate from the identity record)
- **JWT Bearer tokens** carry the following claims:
  - `sub` — the Identity user ID
  - `email`
  - `customerId` — the linked customer's ID (used everywhere in the Order/Payment flow instead of trusting client input)
  - `role` — `Customer` or `Admin`
- **Registration** (`POST /api/auth/register`) atomically creates both the `ApplicationUser` and the linked `Customer` inside a single database transaction — if any step fails, everything rolls back, leaving no orphaned user record
- **Login** (`POST /api/auth/login`) returns the same JWT shape as registration
- Promoting a user to `Admin` (`PUT /api/auth/promote-to-admin/{email}`) is itself an `Admin`-only endpoint — there is no way to self-promote
- **Ownership checks** are consistent across the API: if a customer tries to access another customer's order or payment, the API returns `404 Not Found` (not `403 Forbidden`) — this deliberately avoids confirming to an attacker that the resource exists at all

---

## Order Status Workflow

Order status transitions are governed by an explicit state machine (`AllowedTransitions` dictionary in `OrderService`), not scattered `if` statements:

```
Pending → Processing → Shipped → Delivered
   ↓            ↓
Cancelled   Cancelled
```

- A customer can cancel their own order only while it is still `Pending`
- An Admin can drive general transitions (`Processing → Shipped → Delivered`) or cancel at any earlier stage
- Cancelling an order **restocks** the reserved inventory and **refunds** any redeemed loyalty points, as part of one atomic operation
- Reaching `Delivered` triggers loyalty point accrual and, for Cash orders, automatic payment confirmation

---

## Concurrency & Data Integrity

This is one of the most deliberately engineered parts of the project. Two operations mutate shared inventory state — checkout (decrement) and cancellation (restock) — and both had to be made safe against two requests happening at the exact same moment.

**The problem with the naive approach:** reading a product's stock into memory, checking it, decrementing it, then saving, is *four separate steps*. Two concurrent requests can both read the same "before" value, both pass the check, and both write — resulting in overselling or negative stock.

**The fix:** stock changes are performed as a single atomic SQL statement using EF Core's `ExecuteUpdateAsync`, with the check built directly into the `WHERE` clause:

```sql
UPDATE products
SET quantity_in_stock = quantity_in_stock - @quantity
WHERE product_id = @productId AND quantity_in_stock >= @quantity
```

This executes as one indivisible operation at the database level — there is no window where a second request can observe a stale value. If zero rows are affected, the service knows the stock genuinely wasn't sufficient at that instant, and raises a `ConflictException`.

Because an order can contain multiple products, and a status change can affect stock **and** loyalty points **and** the order record together, these operations are wrapped in explicit database transactions (`BeginTransactionAsync` / `CommitTransactionAsync` / `RollbackTransactionAsync`) at the level of the complete business operation (`CheckoutAsync`, `UpdateStatusAsync`, `CancelOrderAsync`) — never inside a lower-level helper method, so that a partial failure never leaves the database in an inconsistent state (e.g., stock decremented but the order never actually created).

---

## Error Handling

- Three custom exception types, each mapped to an HTTP status code:
  - `NotFoundException` → 404
  - `BadRequestException` → 400
  - `ConflictException` → 409
- A single **global exception handling middleware**, registered first in the pipeline, catches these and converts them into a consistent JSON error response — no repeated try/catch blocks scattered across controllers
- All user-facing error messages (exceptions and validation messages) are written in **Arabic**, matching the target audience of the application

---

## API Endpoints

All routes are prefixed with `/api`. 🔒 = requires a valid JWT. 👑 = requires the `Admin` role.

| Resource | Method & Route | Access |
|---|---|---|
| **Auth** | `POST /auth/register` | Public |
| | `POST /auth/login` | Public |
| | `PUT /auth/promote-to-admin/{email}` | 👑 |
| **Products** | `GET /products`, `GET /products/{id}` | Public |
| | `POST /products` (multipart, image upload) | 👑 |
| | `PUT /products/{id}` | 👑 |
| | `DELETE /products/{id}` | 👑 |
| **Categories** | `GET /categories`, `GET /categories/{id}` | Public |
| | `POST`, `PUT`, `DELETE` | 👑 |
| **Shippers** | `GET /shippers`, `GET /shippers/{id}` | Public |
| | `POST`, `PUT`, `DELETE` | 👑 |
| **Order Statuses** | `GET /orderstatuses`, `GET /orderstatuses/{id}` | Public |
| | `POST`, `PUT`, `DELETE` | 👑 |
| **Customers** | `GET /customer`, `GET /customer/{id}`, `PUT`, `DELETE` | 👑 |
| **Orders** | `POST /orders` (checkout) | 🔒 Customer |
| | `GET /orders/mine` | 🔒 Customer |
| | `GET /orders` (all orders) | 👑 |
| | `GET /orders/{id}` | 🔒 (owner or Admin) |
| | `PUT /orders/{id}/status` | 👑 |
| | `POST /orders/{id}/cancel` | 🔒 (owner while Pending, or Admin anytime) |
| **Payments** | `POST /orders/{orderId}/payment` | 🔒 Customer |
| | `GET /orders/{orderId}/payment` | 🔒 (owner or Admin) |
| | `PUT /orders/{orderId}/payment/status` | 👑 |

List endpoints (`GET` collections) support `pageNumber`, `pageSize`, and resource-specific filter/sort query parameters, and return a `PagedResult<T>` wrapper (not an inherited `List<T>`) to avoid pagination metadata being lost during JSON serialization.

---

## Getting Started

### Prerequisites
- Visual Studio 2026 (or the .NET 10 SDK + any editor)
- SQL Server with the `store_1` database restored/available

### Setup

1. Clone the repository:
   ```
   git clone https://github.com/bulga30000-art/E-commerce.git
   ```
2. Update the connection string in `appsettings.json` to point at your SQL Server instance.
3. Set the JWT signing key via **User Secrets** (do not put it in `appsettings.json`):
   ```
   dotnet user-secrets set "Jwt:Key" "<your-own-strong-random-key>"
   ```
4. Run any pending EF Core migrations:
   ```
   dotnet ef database update
   ```
5. Run the project. Swagger UI is available at `/swagger` in development.
6. Create the first Admin account manually via SQL (add the `Admin` role to an already-registered user in `AspNetUserRoles`), since there is intentionally no public "become admin" endpoint.

---

## Configuration

`appsettings.json` expects:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=store_1;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "",
    "Issuer": "ECommerceApi",
    "Audience": "ECommerceApiUsers",
    "ExpiryMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:3000", "http://localhost:5173" ]
  }
}
```

- `Jwt:Key` is left empty here and supplied via User Secrets locally (or an environment variable in a real deployment) — never committed as plaintext.
- `Cors:AllowedOrigins` lists the front-end origins allowed to call this API from a browser. Add your front-end's dev server URL here if it isn't already listed.

---

## Design Decisions (FAQ)

**Why no generic repository?**
A generic `IRepository<T>` tends to either grow ad-hoc entity-specific methods over time (defeating the point of being generic) or force awkward, overly-generic query methods. A dedicated repository per entity keeps each one's public surface meaningful to that entity specifically.

**Why `PagedResult<T>` instead of inheriting from `List<T>`?**
A type like `PaginatedList<T> : List<T>` serializes to JSON as a bare array, silently dropping pagination metadata (total count, page number, etc.). Wrapping the items in an explicit `PagedResult<T>` object keeps that metadata in the response.

**Why is `NotFoundException` used instead of a `ForbiddenException` when a customer accesses someone else's order?**
Returning `404` instead of `403` avoids confirming to an unauthorized caller that a resource with that ID even exists — a small but meaningful information-disclosure protection.

**Why wasn't a generic Result/Error-handling pattern used instead of exceptions?**
With only three well-defined business error categories (`NotFound`, `BadRequest`, `Conflict`), a global exception middleware maps them to consistent HTTP responses from a single place. A `Result<T>` pattern would move that mapping logic into every controller action individually, working against the thin-controller principle, without a measurable performance benefit at this request volume.

**Why is stock updated via `ExecuteUpdateAsync` instead of loading the entity and saving it?**
See [Concurrency & Data Integrity](#concurrency--data-integrity) — it closes a real race condition where two simultaneous checkouts could both pass a stock check based on a stale in-memory value.
