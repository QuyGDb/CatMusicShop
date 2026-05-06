# API Endpoint Design — Vinyl Shop

## General Conventions

| Item | Convention |
|---|---|
| Base URL | `/api/v1` |
| Auth header | `Authorization: Bearer <jwt_token>` |
| Content-Type | `application/json` |
| ID format | UUID v4 |
| Datetime | ISO 8601 — `2024-01-15T10:30:00Z` |
| Naming | **camelCase** for JSON (request/response) |

### Implementation Sync Notes

Synced with current ASP.NET controllers on 2026-05-06.

- `POST /api/v1/orders` creates the order and Stripe checkout data; there is no standalone `POST /api/v1/payments/stripe/create-session` endpoint.
- `POST /api/v1/payments/stripe/webhook` is the only payment controller endpoint.
- Product details support both slug and GUID routes: `GET /api/v1/products/{slug}` and `GET /api/v1/products/{id}`. The GUID route is public; there is no `/products/admin/{id}` endpoint.
- Standalone track search is not implemented; tracks are exposed through release detail responses.
- Release version detail by ID is not implemented; release versions are listed by release using `GET /api/v1/releaseversions/by-release/{releaseId}`.
- `POST /api/v1/auth/logout` is a public route that invalidates the `refreshToken` cookie when present.

### Permissions

| Role | Condition |
|---|---|
| **Guest** | No JWT, or invalid JWT |
| **Customer** | Valid JWT + `role = customer` |
| **Admin** | Valid JWT + `role = admin` |

### Response Structure

**Success:**
```json
{
  "success": true,
  "data": { ... },
  "meta": {
    "page": 1,
    "limit": 20,
    "total": 120
  }
}
```

**Error:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Product.NotFound",
  "status": 404,
  "detail": "The product was not found.",
  "traceId": "..."
}
```

Controller errors use ASP.NET `ProblemDetails` from `BaseApiController.MapError`, not `ApiResponse<T>`.

### Common Error Codes

| Code | HTTP | Meaning |
|---|---|---|
| `*.Unauthorized` | 401 | Not logged in or invalid credentials |
| `*.Forbidden` | 403 | Insufficient permissions |
| `*.NotFound` | 404 | Resource does not exist |
| `*.Validation` | 400 | Invalid input data |
| `*.Conflict` | 409 | Business conflict, duplicate value, or invalid state |

---

## Section 1 — Authentication

### `POST /api/v1/auth/register`

Register a new account. Default role is `customer`.

**Auth:** Public

**Request body:**
```json
{
  "name": "John Doe",
  "email": "user@example.com",
  "password": "plaintext_password"
}
```

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "accessToken": "jwt_access_token_string",
    "refreshToken": "refresh_token_string",
    "accessTokenExpiresAt": "2024-01-15T10:15:00Z",
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "fullName": "John Doe",
      "role": "Customer",
      "createdAt": "2024-01-15T10:00:00Z"
    }
  }
}
```

### `POST /api/v1/auth/google-login`

Login with Google account. Backend validates `idToken` and returns JWT.

**Auth:** Public

**Request body:**
```json
{
  "idToken": "google_id_token_from_frontend"
}
```

**Response `200`:** Returns an `AuthResponse` object containing JWT and user information.

---

### `POST /api/v1/auth/login`

Login with email/password.

**Auth:** Public

**Request body:**
```json
{
  "email": "user@example.com",
  "password": "plaintext_password"
}
```

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "accessToken": "jwt_access_token_string",
    "refreshToken": "refresh_token_string",
    "accessTokenExpiresAt": "2024-01-15T10:15:00Z",
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "fullName": "John Doe",
      "role": "Customer",
      "createdAt": "2024-01-15T10:00:00Z"
    }
  }
}
```

---

### `POST /api/v1/auth/refresh`

Obtain a new Access Token using Refresh Token from cookie.

**Auth:** Public

**Request:** Refresh Token is automatically retrieved from HttpOnly cookie.

**Response `200`:** Returns a new `AuthResponse` object (similar to `/login`).

---

### `POST /api/v1/auth/logout`

Invalidate current Refresh Token to log out completely. Clears cookie.

**Auth:** Public route; requires `refreshToken` cookie to invalidate a session.

**Request:** Refresh Token is retrieved from cookie.

**Response `200`:**
```json
{ "success": true, "data": null }
```

---

### `GET /api/v1/auth/me`

Get information of the currently logged-in user.

**Auth:** Customer, Admin

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "email": "user@example.com",
    "fullName": "John Doe",
    "role": "Customer",
    "createdAt": "2024-01-15T10:00:00Z"
  }
}
```

---

### `POST /api/v1/auth/change-password`

Change password while logged in.

**Auth:** Customer, Admin

**Request body:**
```json
{
  "currentPassword": "old_password",
  "newPassword": "new_secure_password"
}
```

## Section 2 — Catalog

### `GET /api/v1/artists`

List of artists, supports filtering and pagination.

**Auth:** Public

**Query params:**

| Param | Type | Description |
|---|---|---|
| `genreId` | uuid | Filter by genre |
| `q` | string | Search by name |
| `pageNumber` | int | Page (default: 1) |
| `pageSize` | int | Items per page (default: 20) |

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "name": "Pink Floyd",
      "country": "UK",
      "imageUrl": "https://cdn.example.com/artists/pink-floyd.jpg",
      "genres": [
        { "id": "uuid", "name": "Progressive Rock", "slug": "progressive-rock" }
      ]
    }
  ],
  "meta": { "pageNumber": 1, "pageSize": 20, "maxPage": 5, "totalCount": 85 }
}
```

---

### `GET /api/v1/artists/{slug}`

Artist details, including list of releases.

**Auth:** Public

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "Pink Floyd",
    "slug": "pink-floyd",
    "bio": "...",
    "country": "UK",
    "imageUrl": "https://...",
    "genres": [...],
    "releases": [
      {
        "id": "uuid",
        "title": "The Dark Side of the Moon",
        "year": 1973,
        "coverUrl": "https://..."
      }
    ]
  }
}
```

---

### `POST /api/v1/artists`

Create a new artist.

**Auth:** Admin

**Request body:**
```json
{
  "name": "Miles Davis",
  "bio": "...",
  "country": "US",
  "imageUrl": "https://...",
  "genreIds": ["uuid1", "uuid2"]
}
```

**Response `201`:** Returns the created artist object.

---

### `PUT /api/v1/artists/{id}`

Update artist information. Uses {id:guid} to identify the resource precisely.

**Auth:** Admin

**Request body:** Similar to `POST /artists`.

**Response `200`:** Returns the updated artist object.

---

### `DELETE /api/v1/artists/{id}`

Delete artist (requires {id:guid}). Only possible when no releases are linked.

**Auth:** Admin

**Response `200`:**
```json
{ "success": true, "data": null }
```

---

### `GET /api/v1/genres`

List of all music genres.

**Auth:** Public

---

### `GET /api/v1/genres/{slug}`

Genre details.

**Auth:** Public

**Response `200`:**
```json
{
  "success": true,
  "data": { "id": "uuid", "name": "Progressive Rock", "slug": "progressive-rock" }
}
```

---

### `POST /api/v1/genres`

Create a new genre.

**Auth:** Admin

**Request body:**
```json
{
  "name": "Jazz Fusion",
  "slug": "jazz-fusion"
}
```

**Response `201`:** Returns the created genre object.

---

### `PUT /api/v1/genres/{id}`

Update genre information (uses {id:guid}).

**Auth:** Admin

**Request body:**
```json
{
  "name": "Heavy Metal",
  "slug": "heavy-metal"
}
```

**Response `200`:** Returns the updated genre object.

---

### `DELETE /api/v1/genres/{id}`

Delete genre (uses {id:guid}). Only performed when no Artist or Release is linked.

**Auth:** Admin

**Response `200`:**
```json
{ "success": true, "data": null }
```

---

### `GET /api/v1/releases`

List of releases, supports filtering.

**Auth:** Public

**Query params:**

| Param | Type | Description |
|---|---|---|
| `artistId` | uuid | Filter by artist |
| `genreId` | uuid | Filter by genre |
| `format` | string | Format (vinyl, cd, cassette) |
| `q` | string | Search by name |
| `pageNumber` | int | Page (default: 1) |
| `pageSize` | int | Items per page (default: 20) |

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "title": "The Dark Side of the Moon",
      "year": 1973,
      "coverUrl": "https://...",
      "artist": { "id": "uuid", "name": "Pink Floyd" },
      "genres": [...]
    }
  ],
  "meta": { "pageNumber": 1, "pageSize": 20, "maxPage": 12, "totalCount": 240 }
}
```

---

### `GET /api/v1/releases/{slug}`

Release details, including tracklist and list of pressing versions.

**Auth:** Public

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "title": "The Dark Side of the Moon",
    "year": 1973,
    "coverUrl": "https://...",
    "artist": { "id": "uuid", "name": "Pink Floyd" },
    "genres": [...],
    "tracks": [
      {
        "id": "uuid",
        "position": 1,
        "title": "Speak to Me",
        "durationSeconds": 68,
        "side": "A"
      }
    ],
    "versions": [
      {
        "id": "uuid",
        "pressingCountry": "UK",
        "pressingYear": 1973,
        "format": "vinyl",
        "catalogNumber": "SHVL 804",
        "label": { "id": "uuid", "name": "Harvest Records" },
        "notes": "First UK pressing"
      }
    ]
  }
}
```

---

### `POST /api/v1/releases`

Create a new release.

**Auth:** Admin

**Request body:**
```json
{
  "artistId": "uuid",
  "title": "Wish You Were Here",
  "year": 1975,
  "coverUrl": "https://...",
  "genreIds": ["uuid1"],
  "tracks": [
    {
      "position": 1,
      "title": "Shine On You Crazy Diamond (Parts I-V)",
      "durationSeconds": 817,
      "side": "A"
    }
  ]
}
```

**Response `201`:** Returns the created release object (with tracks).

---

### `PUT /api/v1/releases/{id}`

Update release.

**Auth:** Admin

---

### `DELETE /api/v1/releases/{id}`

Delete release.

**Auth:** Admin

**Response `204`:**

---

### `GET /api/v1/releaseversions/by-release/{releaseId}`

List of pressing versions for a release.

**Auth:** Public

---

### `GET /api/v1/releaseversions/formats`

List of supported formats.

**Auth:** Public

---

### `POST /api/v1/releaseversions`

Create a new pressing version for a release.

**Auth:** Admin

**Request body:**
```json
{
  "releaseId": "uuid",
  "labelId": "uuid",
  "pressingCountry": "Japan",
  "catalogNumber": "EMS-80324",
  "pressingYear": 1976,
  "format": "vinyl",
  "notes": "OBI strip, first Japan pressing"
}
```

**Response `201`:** Returns the created release_version object.

---

### `PUT /api/v1/releaseversions/{id}`

Update pressing version.

**Auth:** Admin

---

### `DELETE /api/v1/releaseversions/{id}`

Delete pressing version. Cannot be deleted if a `Product` is linked.

**Auth:** Admin

**Response `200`:**
```json
{ "success": true, "data": null }
```

---

### `GET /api/v1/labels`

List of record labels.

**Auth:** Public

**Query params:** `q` (search by name), `country`, `page`, `limit`

---

### `GET /api/v1/labels/{slug}`

Label details.

**Auth:** Public

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "Harvest Records",
    "country": "UK",
    "foundedYear": 1969,
    "website": "https://..."
  }
}
```

---

### `POST /api/v1/labels`

Create a new label.

**Auth:** Admin

**Request body:**
```json
{
  "name": "Harvest Records",
  "country": "UK",
  "foundedYear": 1969,
  "website": "https://..."
}
```

**Response `201`:** Returns the created label.

---

### `PUT /api/v1/labels/{id}`

Update label (uses {id:guid}).

**Auth:** Admin

---

### `DELETE /api/v1/labels/{id}`

Delete label (uses {id:guid}). Only performed when no release version is linked.

**Auth:** Admin

**Response `200`:**
```json
{ "success": true, "data": null }
```

---

Standalone track search is not implemented in the current API. Track data is returned through release details.

---

## Section 3 — Products

### `GET /api/v1/products`

List of products for sale (`is_active = true`). Admin can see hidden products as well.

**Auth:** Public

**Query params:**

| Param | Type | Description |
|---|---|---|
| `format` | enum | `vinyl` / `cd` / `cassette` |
| `genre` | string | Slug of genre |
| `artistId` | uuid | Filter by artist |
| `isLimited` | boolean | Show only limited items |
| `isPreorder` | boolean | Show only pre-order items |
| `minPrice` | decimal | Minimum price |
| `maxPrice` | decimal | Maximum price |
| `searchQuery` | string | Search by product name |
| `pageNumber` | int | Page (default: 1) |
| `pageSize` | int | Items per page (default: 20) |

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "name": "Pink Floyd — The Dark Side of the Moon",
      "format": "vinyl",
      "isLimited": false,
      "isPreorder": false,
      "isActive": true,
      "coverUrl": "https://...",
      "artist": { "id": "uuid", "name": "Pink Floyd" },
      "minPrice": 1200000,
      "maxPrice": 2500000,
      "inStock": true
    }
  ],
  "meta": { "pageNumber": 1, "pageSize": 20, "maxPage": 10, "totalCount": 180 }
}
```

> `min_price` and `max_price` are calculated from the list of variants. `in_stock` = true when at least 1 variant has `stock_qty > 0`.

---

### `GET /api/v1/products/{slug}`

Product details with release information.

**Auth:** Public

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "Pink Floyd — The Dark Side of the Moon (Japan OBI)",
    "slug": "pink-floyd-dark-side-japan-obi",
    "description": "...",
    "format": "vinyl",
    "isLimited": true,
    "limitedQty": 50,
    "isPreorder": false,
    "price": 1200000,
    "stockQty": 10,
    "isAvailable": true,
    "isSigned": false,
    "artist": { "id": "uuid", "name": "Pink Floyd" },
    "vinylAttributes": {
      "discColor": "black",
      "weightGrams": "140",
      "speedRpm": "33",
      "discCount": "1lp",
      "sleeveType": "standard"
    }
  }
}
```

---

### `GET /api/v1/products/{id}`

Product details by ID. This route is constrained to GUIDs in the controller and shares the same response shape as `GET /api/v1/products/{slug}`.

**Auth:** Public

---

### `POST /api/v1/products`

Create a new product from a release_version.

**Auth:** Admin

**Request body:**
```json
{
  "releaseVersionId": "uuid",
  "name": "Pink Floyd — The Dark Side of the Moon (Japan OBI)",
  "description": "...",
  "isLimited": true,
  "limitedQty": 50,
  "isPreorder": false
}
```

**Response `201`:** Returns the created product object (without variants yet).

---

### `PATCH /api/v1/products/{id}`

Update product information.

**Auth:** Admin

**Request body:**
```json
{
  "slug": "...",
  "description": "...",
  "isActive": true,
  "isPreorder": false,
  "price": 1200000,
  "stockQty": 10,
  "vinylAttributes": { ... }
}
```

---

### `DELETE /api/v1/products/{id}`

Deactivate product (set `isActive = false`).

**Auth:** Admin

**Response `204`:**

---

### `GET /api/v1/curatedcollections`

List of themed collections.

**Auth:** Public

**Query params:** `includeUnpublished`, `pageNumber`, `pageSize`, `searchQuery`

---

### `GET /api/v1/curatedcollections/featured`

Get a list of featured collections for the homepage.

**Auth:** Public

---

### `GET /api/v1/curatedcollections/{id}`

Collection details.

**Auth:** Public

---

### `POST /api/v1/curatedcollections`

Create a new collection.

**Auth:** Admin

---

### `PATCH /api/v1/curatedcollections/{id}`

Update collection information.

**Auth:** Admin

---

### `PUT /api/v1/curatedcollections/{id}/status`

Update display status (isPublished).

**Auth:** Admin

---

### `POST /api/v1/curatedcollections/{id}/items`

Add a product to the collection.

**Auth:** Admin

**Request body:**
```json
{
  "productId": "uuid",
  "sortOrder": 1
}
```

---

### `DELETE /api/v1/curatedcollections/{id}/items/{productId}`

Remove a product from the collection.

**Auth:** Admin

---

### `DELETE /api/v1/curatedcollections/{id}`

Delete collection.

**Auth:** Admin

---

## Section 4 — Cart

### `GET /api/v1/cart`

Get the current user's cart.

**Auth:** Customer

---

### `POST /api/v1/cart/items`

Add a product to the cart.

**Auth:** Customer

**Request body:**
```json
{
  "productId": "uuid",
  "quantity": 1
}
```

**Response `200`:** Returns the ID of the newly added/updated item record.

---

### `PUT /api/v1/cart/items/{id}`

Update product quantity in the cart.

**Auth:** Customer

**Request body:**
```json
{
  "quantity": 3
}
```

---

### `DELETE /api/v1/cart/items/{id}`

Remove a product from the cart.

**Auth:** Customer

---

### `DELETE /api/v1/cart`

Clear the entire cart.

**Auth:** Customer

---

## Section 5 — Orders

### `POST /api/v1/orders`

Create an order from the current cart. Performed within a single database transaction.

**Auth:** Customer

**Request body:**
```json
{
  "shippingAddress": "123 Nguyen Hue, Q1, TP.HCM",
  "recipientName": "John Doe",
  "phone": "0901234567",
  "paymentMethod": "stripe"
}
```

**Internal Logic (service layer):**

1. Check `stock_qty` sufficiency for each variant
2. If `is_preorder = true`, skip stock deduction until `preorder_release_date`
3. Snapshot price into `order_items.unit_price`
4. Snapshot shipping information into `orders`
5. Calculate `total_amount` from `order_items`
6. Deduct `stock_qty` (if not a preorder)
7. Clear items from the cart
8. Create a `payments` record with `status = pending`
9. Trigger `order_created` email sending

**Response `201`:**
```json
{
  "success": true,
  "data": {
    "order": {
      "id": "uuid",
      "status": "pending",
      "totalAmount": 2400000,
      "createdAt": "2024-01-15T10:00:00Z"
    },
    "payment": {
      "id": "uuid",
      "method": "stripe",
      "status": "pending",
      "stripeCheckoutUrl": "https://checkout.stripe.com/..."
    }
  }
}
```

---

### `GET /api/v1/orders`

Current user's order history.

**Auth:** Customer

**Query params:** `status`, `page`, `limit`

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "status": "completed",
      "totalAmount": 2400000,
      "createdAt": "2024-01-15T10:00:00Z",
      "itemCount": 2
    }
  ],
  "meta": { "pageNumber": 1, "pageSize": 20, "maxPage": 1, "totalCount": 5 }
}
```

---

### `GET /api/v1/orders/{id}`

Order details including order items and payment information.

**Auth:** Customer (view own orders only), Admin (view all)

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "status": "shipped",
    "shippingAddress": "123 Nguyen Hue, Q1, TP.HCM",
    "recipientName": "John Doe",
    "phone": "0901234567",
    "totalAmount": 2400000,
    "trackingNumber": "VN123456789",
    "createdAt": "2024-01-15T10:00:00Z",
    "items": [
      {
        "id": "uuid",
        "quantity": 2,
        "unitPrice": 1200000,
        "subtotal": 2400000,
        "variant": {
          "id": "uuid",
          "variantName": "Black 140g Standard",
          "product": {
            "id": "uuid",
            "name": "Pink Floyd — The Dark Side of the Moon",
            "coverUrl": "https://..."
          }
        }
      }
    ],
    "payment": {
      "method": "vnpay",
      "status": "success",
      "paidAt": "2024-01-15T10:05:00Z"
    }
  }
}
```

---

### `POST /api/v1/orders/{id}/cancel`

Cancel an order.

**Auth:** Customer, Admin

**Response `204`:**

---

### `GET /api/v1/admin/orders`

Admin view of all orders in the system.

**Auth:** Admin

**Query params:** `status`, `userId`, `pageNumber`, `pageSize`

---

### `PATCH /api/v1/admin/orders/{id}/status`

Admin updates order status.

**Auth:** Admin

**Request body:**
```json
{
  "status": "shipped",
  "trackingNumber": "VN123456789"
}
```


---

## Section 6 — Payment

Stripe Checkout Session creation is handled by `POST /api/v1/orders`. There is no standalone `create-session` endpoint in the current API.

### `POST /api/v1/payments/stripe/webhook`

Stripe Webhook — server-to-server notification. Verify `Stripe-Signature` signature and update order status.

**Auth:** Public (Verify using Stripe Webhook Secret)

**Response `204`:**

---

## Section 7 — Catalog Search

### `GET /api/v1/catalog/search`

Search for artists and releases by keyword.

**Auth:** Public

**Query params:** `q` (keyword)

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "artists": [...],
    "releases": [...]
  }
}
```

---

## Section 8 — Uploads

### `POST /api/v1/uploads/image`

Upload images to the server.

**Auth:** Admin

**Request body:** `multipart/form-data` with `file` and `folder`.

**Response `200`:** Returns the URL of the image.

---

## Summary Table

| # | Method | Endpoint | Auth |
|---|---|---|---|
| 1 | POST | `/api/v1/auth/register` | Public |
| 2 | POST | `/api/v1/auth/login` | Public |
| 3 | POST | `/api/v1/auth/google-login` | Public |
| 4 | POST | `/api/v1/auth/refresh` | Public |
| 5 | GET | `/api/v1/auth/me` | Authenticated |
| 6 | POST | `/api/v1/auth/logout` | Public route; refresh cookie required |
| 7 | POST | `/api/v1/auth/change-password` | Authenticated |
| 8 | GET | `/api/v1/artists` | Public |
| 9 | GET | `/api/v1/artists/{slug}` | Public |
| 10 | POST | `/api/v1/artists` | Admin |
| 11 | PUT | `/api/v1/artists/{id}` | Admin |
| 12 | DELETE | `/api/v1/artists/{id}` | Admin |
| 13 | GET | `/api/v1/genres` | Public |
| 14 | GET | `/api/v1/genres/{slug}` | Public |
| 15 | POST | `/api/v1/genres` | Admin |
| 16 | PUT | `/api/v1/genres/{id}` | Admin |
| 17 | DELETE | `/api/v1/genres/{id}` | Admin |
| 18 | GET | `/api/v1/releases` | Public |
| 19 | GET | `/api/v1/releases/{slug}` | Public |
| 20 | POST | `/api/v1/releases` | Admin |
| 21 | PUT | `/api/v1/releases/{id}` | Admin |
| 22 | DELETE | `/api/v1/releases/{id}` | Admin |
| 23 | GET | `/api/v1/releaseversions/by-release/{releaseId}` | Public |
| 24 | GET | `/api/v1/releaseversions/formats` | Public |
| 25 | POST | `/api/v1/releaseversions` | Admin |
| 26 | PUT | `/api/v1/releaseversions/{id}` | Admin |
| 27 | DELETE | `/api/v1/releaseversions/{id}` | Admin |
| 28 | GET | `/api/v1/labels` | Public |
| 29 | GET | `/api/v1/labels/{slug}` | Public |
| 30 | POST | `/api/v1/labels` | Admin |
| 31 | PUT | `/api/v1/labels/{id}` | Admin |
| 32 | DELETE | `/api/v1/labels/{id}` | Admin |
| 33 | GET | `/api/v1/products` | Public |
| 34 | GET | `/api/v1/products/{slug}` | Public |
| 35 | GET | `/api/v1/products/{id}` | Public |
| 36 | POST | `/api/v1/products` | Admin |
| 37 | PATCH | `/api/v1/products/{id}` | Admin |
| 38 | DELETE | `/api/v1/products/{id}` | Admin |
| 39 | GET | `/api/v1/curatedcollections` | Public |
| 40 | GET | `/api/v1/curatedcollections/featured` | Public |
| 41 | GET | `/api/v1/curatedcollections/{id}` | Public |
| 42 | POST | `/api/v1/curatedcollections` | Admin |
| 43 | PATCH | `/api/v1/curatedcollections/{id}` | Admin |
| 44 | PUT | `/api/v1/curatedcollections/{id}/status` | Admin |
| 45 | POST | `/api/v1/curatedcollections/{id}/items` | Admin |
| 46 | DELETE | `/api/v1/curatedcollections/{id}/items/{productId}` | Admin |
| 47 | DELETE | `/api/v1/curatedcollections/{id}` | Admin |
| 48 | GET | `/api/v1/cart` | Authenticated |
| 49 | POST | `/api/v1/cart/items` | Authenticated |
| 50 | PUT | `/api/v1/cart/items/{id}` | Authenticated |
| 51 | DELETE | `/api/v1/cart/items/{id}` | Authenticated |
| 52 | DELETE | `/api/v1/cart` | Authenticated |
| 53 | GET | `/api/v1/orders` | Authenticated |
| 54 | GET | `/api/v1/orders/{id}` | Authenticated |
| 55 | POST | `/api/v1/orders` | Authenticated |
| 56 | POST | `/api/v1/orders/{id}/cancel` | Authenticated |
| 57 | GET | `/api/v1/admin/orders` | Admin |
| 58 | PATCH | `/api/v1/admin/orders/{id}/status` | Admin |
| 59 | POST | `/api/v1/payments/stripe/webhook` | Public |
| 60 | GET | `/api/v1/catalog/search` | Public |
| 61 | POST | `/api/v1/uploads/image` | Admin |

**Total: 61 endpoints**
