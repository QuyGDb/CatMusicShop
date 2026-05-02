# MusicShop — Business Documentation

> Music Shop · ASP.NET Core 10

MusicShop is an online platform for selling physical media (vinyl, CD, cassette). The Admin is the sole manager of the catalog. The key differentiator is the detailed catalog management and specialized attributes for different physical media formats.

---

## Table of Contents

0. [Feature Overview](#0-feature-overview)
1. [User Roles](#1-user-roles)
2. [Music Catalog](#2-music-catalog)
3. [Products & Sales](#3-products--sales)
4. [Order Process](#4-order-process)
5. [Payments](#5-payments)
6. [Notifications & Business Events](#6-notifications--business-events)

---

## 0. Feature Overview

### 0.1 Authentication & Accounts

- User registration (Local)
- Login / logout (Local + Google Auth)
- Role-based access control (Guest / Customer / Admin)

### 0.2 Catalog & Products

- Browse products: general, random
- Filter products (music genre, format (CD, cassette, vinyl), artist, country, price, decade)
- Search products (album name, single, EP, track name, music genre, format (CD, cassette, vinyl), artist, country)
- View product details (artist info, edition, tracklist)
- Browse products by curated collections
- Catalog management (Admin: add / edit / delete artists, products)

### 0.3 Shopping

- Add products to cart
- Place orders
- Choose payment method (Stripe)
- Pre-order items
- Track order status
- Cancel orders (Customer: only when Pending / Admin: from Confirmed onward)
- Review products after order completion

### 0.4 Payments

- Online payment via Stripe gateway
- Transaction history recording

### 0.5 Order Management (Admin)

- Confirm orders
- Update shipping status
- Cancel orders
- View revenue reports

### 0.6 Notifications

- Send confirmation email when an order is placed
- Send email when an order is confirmed
- Send email with shipping information when an order is shipped
- Send review invitation email when an order is completed
- Send cancellation notification email when an order is cancelled

---

## 1. User Roles

| Role | Description | Permissions |
|---|---|---|
| **Guest** | Unauthenticated visitor | Browse catalog, search & filter products |
| **Customer** | Registered user (Email or Google) | Purchase items, track orders |
| **Admin** | Shop owner | Manage catalog, confirm / ship / cancel orders, view revenue reports, view action logs |

---

## 2. Music Catalog

The music data store is completely separate from the buying/selling logic. This serves as the foundation for AI functionality and for accurate product information management.

### 2.1 Data Hierarchy

```
Artist  ──→  Original Release  ──→  Specific Edition  ──→  Tracklist
```

| Entity | Stored Information | Examples |
|---|---|---|
| **Artist** | Name, biography, genre, country | Pink Floyd, Miles Davis |
| **Original Release** | Album name, release year, genre, cover art | "Dark Side of the Moon" (1973) |
| **Specific Edition** | Pressing number, country, format, catalog number, record label | US first press, Japan obi 1976, 2011 Remaster |
| **Tracklist** | Position, track name, duration | Side A — Track 1: Speak to Me |

### 2.2 Why Separate Original Release from Specific Edition

A single original album can have dozens of different editions, each with its own price, condition, and inventory. This hierarchy enables accurate management of each individual pressing without duplicating artistic information.

### 2.3 Record Labels

Stores record label information linked to each specific edition: label name, country, founding year, website.

---

## 3. Products & Sales

### 3.1 Product Types

- Vinyl
- CD
- Cassette

### 3.2 Product Variants

Each product can have multiple variants. Each variant has its own independent price and inventory.

**Vinyl**

| Group | Examples |
|---|---|
| Disc color | Black, Colored, Splatter, Picture disc |
| Weight | 140g (standard), 180g (audiophile) |
| Speed | 33⅓ RPM, 45 RPM |
| Disc count | 1LP, 2LP, Box set |
| Sleeve edition | Standard sleeve, Gatefold, Obi strip |
| Autograph | Signed by artist |

**CD**

| Group | Examples |
|---|---|
| Content | Standard, Deluxe / Expanded (bonus tracks), Box set |
| Regional edition | Japan edition (often includes exclusive bonus tracks) |
| Autograph | Signed by artist |

**Cassette**

| Group | Examples |
|---|---|
| Tape color | Black, Clear, White, Colored |
| Edition | Standard, Limited edition |
| Autograph | Signed by artist |

### 3.3 Special Features

**Limited Edition**

- A fixed quantity is determined before sales begin.
- The limited quantity may not be increased once sales have started.

**Pre-order**

- Customers place orders before the official release date.
- Inventory is not decremented until the actual release date arrives.

### 3.4 Inventory Rules

- Out of stock → automatically marked as unavailable for purchase, hidden from in-stock listings.
- Products cannot be deleted while there are orders in Pending or Confirmed status.
- When stock is replenished → the system automatically notifies customers who have that product in their Wantlist.

### 3.5 Curated Collections

The Admin can create editorially themed collections.

Examples: *Horror Soundtracks*, *Video Game OST*, *Vietnam New Wave*.

---

## 4. Order Process

### 4.1 Order Lifecycle

```
Pending → Confirmed → Shipped → Delivered → Completed
             ↓
          Cancelled (Admin only, from this state onward)
```

| Status | Description |
|---|---|
| **Pending** | Customer just placed the order, awaiting confirmation |
| **Confirmed** | Admin has confirmed the order |
| **Shipped** | Order has been dispatched |
| **Delivered** | Order has reached the customer |
| **Completed** | Fully complete; customer may leave a review |
| **Cancelled** | Order has been cancelled |

### 4.2 Business Rules

- **Checkout:** inventory is decremented immediately upon successful order placement.
- **Cancellation at Pending:** full inventory is restored to its previous level.
- **Cancellation from Confirmed onward:** only the Admin has the authority to do this.
- **Each order has exactly one payment.**

---

## 5. Payments

### 5.1 Payment Methods

| Method | Description |
|---|---|
| **Stripe** | Online payment via Stripe gateway |

### 5.2 Recorded Information

Each payment transaction stores: amount, payment method, transaction ID, timestamp, and status.

---

## 6. Notifications & Business Events

### 6.1 Order Lifecycle Notifications

| Event | Notification Action |
|---|---|
| Order placed | Send order confirmation email to customer |
| Order confirmed | Send status update email |
| Order shipped | Send email with shipping information |
| Order completed | Send review invitation email to customer |
| Order cancelled | Send cancellation notification email |