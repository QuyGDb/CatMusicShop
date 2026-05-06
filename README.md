# 🎵 CatMusicShop

A full-stack e-commerce platform for selling physical music media — **Vinyl**, **CD**, and **Cassette**. Built with Clean Architecture principles, featuring a rich music catalog, Stripe payments, and an interactive 3D storefront.

> **Live:** [catmusicshop.duckdns.org](https://catmusicshop.duckdns.org)
<br/>
![CatMusicShop Showcase](./overview.gif)

---

## Documentation

| Document | Description |
|---|---|
| [Business Overview](docs/overview.md) | Feature requirements and business rules |
| [ERD Analysis](docs/erd-analysis-en.md) | Full entity-relationship design with rationale |
| [API Endpoint Design](docs/api-endpoint-design.md) | REST endpoint specifications |

---

## Tech Stack

### Backend

| Layer | Technology |
|---|---|
| Runtime | .NET 10, C# 12 |
| Architecture | Clean Architecture (Domain → Application → Infrastructure → API) |
| API | RESTful Controllers, API Versioning (`/api/v1/`) |
| Database | PostgreSQL (pgvector) |
| ORM | Entity Framework Core 10 |
| Auth | JWT + Refresh Tokens, Google OAuth |
| Payments | Stripe (Checkout Sessions + Webhooks) |
| Storage | AWS S3 + CloudFront CDN |
| Email | MailKit (SMTP) |
| Background Jobs | Hangfire |
| Messaging | Outbox Pattern (reliable event delivery) |
| Logging | Serilog (structured, file sink) |
| Validation | FluentValidation |
| CQRS | MediatR |

### Frontend

| Category | Technology |
|---|---|
| Framework | React 19, TypeScript 5, Vite 8 |
| Styling | TailwindCSS 4 + shadcn/ui |
| State | TanStack Query (server) + Zustand (client) |
| Forms | React Hook Form + Zod |
| 3D | Three.js + React Three Fiber + Drei |
| Routing | React Router v7 |
| HTTP | Axios |
| Testing | Vitest + Playwright + Storybook |

### Infrastructure

| Component | Technology |
|---|---|
| Containerization | Docker + Docker Compose |
| Reverse Proxy | Nginx + Let's Encrypt SSL |
| Hosting | AWS EC2 |
| CI/CD | GitHub Actions |

---

## Architecture

```
MusicShop/
├── src/
│   ├── MusicShop.Domain/           # Entities, Enums, Interfaces (zero dependencies)
│   ├── MusicShop.Application/      # Use Cases (CQRS), DTOs, Validators, Events
│   ├── MusicShop.Infrastructure/   # EF Core, S3, Stripe, Email, Outbox Messaging
│   ├── MusicShop.API/              # Controllers, Middleware, DI wiring
│   └── musicshop-web/              # React SPA
├── tests/
├── docs/
├── docker-compose.yml
└── docker-compose.prod.yml
```

### Domain Modules

| Module | Entities |
|---|---|
| **Catalog** | Artist, Genre, Release, ReleaseVersion, Label, Track |
| **Shop** | Product, VinylAttributes, CdAttributes, CassetteAttributes, CuratedCollection |
| **Orders** | Cart, CartItem, Order, OrderItem |
| **Payments** | Payment |
| **Messaging** | OutboxMessage (reliable event delivery) |
| **Auth** | User, RefreshToken |

---

## Features

### Storefront (Customer)

- Browse and filter products by genre, format, artist, country, price, and decade
- Full-text catalog search across albums, tracks, artists, and genres
- Interactive 3D hero scene (Three.js)
- Shopping cart with persistent state
- Stripe checkout with order confirmation emails
- Order tracking and history
- Google OAuth login

### Admin Dashboard

- Full CRUD for artists, genres, labels, releases, release versions, and products
- Format-specific attribute management (vinyl color/weight, CD edition, cassette tape color)
- Curated collection editor with custom sort order
- Order management: confirm → ship → deliver → complete
- Order cancellation with reason tracking
- Image uploads to S3

### Technical Highlights

- **Outbox Pattern** — Domain events are persisted in a transactional outbox and processed by Hangfire, ensuring reliable email delivery even if the mail server is temporarily down
- **Snapshot-based Orders** — Prices, addresses, and recipient info are captured at checkout time, immune to future data changes
- **Format Polymorphism** — Vinyl, CD, and Cassette each have dedicated attribute tables linked 1:1 to products, avoiding nullable-column bloat
- **Catalog ↔ Sales Separation** — Music metadata (artist, release, tracklist) is cleanly separated from sales data (price, stock, availability)

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Local Development (Docker Compose)

1. Clone the repository:

```bash
git clone https://github.com/your-username/CatMusicShop.git
cd CatMusicShop/MusicShop
```

2. Create a `.env` file from the template and fill in your credentials:

```env
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password
POSTGRES_DB=catmusicshop
DB_CONNECTION_STRING=Host=postgres;Port=5432;Database=catmusicshop;Username=postgres;Password=your_password

# App
ASPNETCORE_ENVIRONMENT=Development

# JWT
JWT_SECRET=your_jwt_secret_at_least_32_chars
JWT_ISSUER=CatMusicShop
JWT_AUDIENCE=CatMusicShop

# Google OAuth
GOOGLE_CLIENT_ID=your_google_client_id

# AWS S3
AWS__BucketName=your_bucket
AWS__Region=ap-southeast-1
AWS__CdnBaseUrl=https://your-cdn.cloudfront.net
AWS__AccessKey=your_access_key
AWS__SecretKey=your_secret_key

# Stripe
STRIPE_SECRET_KEY=sk_test_...
STRIPE_PUBLISHABLE_KEY=pk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...

# Email (SMTP)
EMAIL_USER=your_email
EMAIL_PASS=your_app_password
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_FROM=CatMusicShop

# Admin Seed
AdminSettings__Email=admin@catmusicshop.com
AdminSettings__Password=YourAdminPassword
AdminSettings__FullName=Admin
```

3. Start all services:

```bash
docker compose up --build
```

4. Access the app:

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000/api/v1 |
| Swagger | http://localhost:5000/swagger |

### Without Docker

**Backend:**

```bash
cd MusicShop/src/MusicShop.API
dotnet run
```

**Frontend:**

```bash
cd MusicShop/src/musicshop-web
npm install
npm run dev
```

---

## API Endpoints

| Resource | Methods | Auth |
|---|---|---|
| `/api/v1/auth` | Register, Login, Google, Refresh, Logout | Public / Authenticated |
| `/api/v1/artists` | CRUD | Public (read) / Admin (write) |
| `/api/v1/genres` | CRUD | Public (read) / Admin (write) |
| `/api/v1/labels` | CRUD | Public (read) / Admin (write) |
| `/api/v1/releases` | CRUD | Public (read) / Admin (write) |
| `/api/v1/release-versions` | CRUD | Public (read) / Admin (write) |
| `/api/v1/products` | CRUD + Search + Filter | Public (read) / Admin (write) |
| `/api/v1/curated-collections` | CRUD | Public (read) / Admin (write) |
| `/api/v1/cart` | Get, Add, Update, Remove, Clear | Customer |
| `/api/v1/orders` | Place, List, Details, Cancel, Update Status | Customer / Admin |
| `/api/v1/payments` | Create Checkout Session, Webhook | Customer / Stripe |
| `/api/v1/uploads` | Upload Image | Admin |
| `/api/v1/catalog` | Aggregated Search | Public |

---

## License

This project is for educational and portfolio purposes.
