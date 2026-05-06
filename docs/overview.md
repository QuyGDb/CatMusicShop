# MusicShop — Business Documentation

> Music Shop · ASP.NET Core 10

MusicShop is an online platform for selling **Physical Media** (**Vinyl**, **CD**, **Cassette**). The **Admin** is the sole manager of the **Catalog**. The key differentiator is detailed **Catalog** management and specialized **Attributes** for different **Physical Media Formats**.

> **Bold terms** mark entity-candidate keywords for ERD/domain modeling.

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

- **User** registration (Local)
- Login / logout (Local + Google Auth)
- **Role**-based access control (**Guest** / **Customer** / **Admin**)

### 0.2 Catalog & Products

- Browse **Products**: general, random
- Filter **Products** (**Genre**, **Format**: **CD**, **Cassette**, **Vinyl**, **Artist**, **Country**, **Price**, **Decade**)
- Search **Products** (**Album**, **Single**, **EP**, **Track**, **Genre**, **Format**, **Artist**, **Country**)
- View **Product** details (**Artist** info, **Edition**, **Tracklist**)
- Browse **Products** by **Curated Collections**
- **Catalog** management (**Admin**: add / edit / delete **Artists**, **Products**)

### 0.3 Shopping

- Add **Products** to **Cart**
- Place **Orders**
- Choose **Payment Method** (**Stripe**)
- **Pre-order** items
- Track **Order Status**
- Cancel **Orders** (**Customer**: only when **Pending** / **Admin**: from **Confirmed** onward)
- **Review** **Products** after **Order** completion

### 0.4 Payments

- Online **Payment** via **Stripe** gateway
- **Transaction** history recording

### 0.5 Order Management (Admin)

- Confirm **Orders**
- Update **Shipping Status**
- Cancel **Orders**
- View **Revenue Reports**

### 0.6 Notifications

- Send **Confirmation Email** when an **Order** is placed
- Send **Status Email** when an **Order** is confirmed
- Send **Shipping Email** with **Shipping Information** when an **Order** is shipped
- Send **Review Invitation Email** when an **Order** is completed
- Send **Cancellation Email** when an **Order** is cancelled

---

## 1. User Roles

| Role | Description | Permissions |
|---|---|---|
| **Guest** | Unauthenticated visitor | Browse catalog, search & filter products |
| **Customer** | Registered user (Email or Google) | Purchase items, track orders |
| **Admin** | Shop owner | Manage catalog, confirm / ship / cancel orders, view revenue reports, view action logs |

---

## 2. Music Catalog

The **Music Catalog** data store is completely separate from the buying/selling logic. This serves as the foundation for AI functionality and for accurate **Product** information management.

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

A single original **Album** can have dozens of different **Editions**, each with its own **Price**, **Condition**, and **Inventory**. This hierarchy enables accurate management of each individual **Pressing** without duplicating **Artist** information.

### 2.3 Record Labels

Stores **Record Label** information linked to each specific **Edition**: **Label Name**, **Country**, **Founding Year**, **Website**.

---

## 3. Products & Sales

### 3.1 Product Types

- **Vinyl**
- **CD**
- **Cassette**

### 3.2 Product Variants

Each **Product** can have multiple **Variants**. Each **Variant** has its own independent **Price** and **Inventory**.

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
- The **Limited Quantity** may not be increased once **Sales** have started.

**Pre-order**

- **Customers** place **Orders** before the official **Release Date**.
- **Inventory** is not decremented until the actual **Release Date** arrives.

### 3.4 Inventory Rules

- **Out of Stock** → automatically marked as unavailable for purchase, hidden from in-stock listings.
- **Products** cannot be deleted while there are **Orders** in **Pending** or **Confirmed** status.
- When **Stock** is replenished → the system automatically notifies **Customers** who have that **Product** in their **Wantlist**.

### 3.5 Curated Collections

The **Admin** can create editorially themed **Collections**.

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
| **Pending** | **Customer** just placed the **Order**, awaiting confirmation |
| **Confirmed** | **Admin** has confirmed the **Order** |
| **Shipped** | **Order** has been dispatched |
| **Delivered** | **Order** has reached the **Customer** |
| **Completed** | Fully complete; **Customer** may leave a **Review** |
| **Cancelled** | **Order** has been cancelled |

### 4.2 Business Rules

- **Checkout:** **Inventory** is decremented immediately upon successful **Order** placement.
- **Cancellation at Pending:** full **Inventory** is restored to its previous level.
- **Cancellation from Confirmed onward:** only the **Admin** has the authority to do this.
- **Each Order has exactly one Payment.**

---

## 5. Payments

### 5.1 Payment Methods

| Method | Description |
|---|---|
| **Stripe** | Online payment via Stripe gateway |

### 5.2 Recorded Information

Each **Payment Transaction** stores: **Amount**, **Payment Method**, **Transaction ID**, **Timestamp**, and **Status**.

---

## 6. Notifications & Business Events

**Notifications** are sent as application side effects; the current schema does not store **Notification History**.

### 6.1 Order Lifecycle Notifications

| Event | Notification Action |
|---|---|
| **Order** placed | Send **Order Confirmation Email** to **Customer** |
| **Order** confirmed | Send **Status Update Email** |
| **Order** shipped | Send **Shipping Email** with **Shipping Information** |
| **Order** completed | Send **Review Invitation Email** to **Customer** |
| **Order** cancelled | Send **Cancellation Email** |
