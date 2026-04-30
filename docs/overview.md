# MusicShop — Complete Project Documentation

---

## 1. Project Overview

> Music Shop · ASP.NET Core 10

### 1.1 Overview
MusicShop is an online platform for selling physical media (vinyl, CD, cassette). The Admin is the sole manager of the catalog.

The key differentiator is the detailed catalog management and specialized attributes for different physical media formats.


---

# MusicShop — Tài liệu Nghiệp vụ

---

## Mục lục

0. [Tổng quan tính năng](#0-tổng-quan-tính-năng)
1. [Vai trò người dùng](#1-vai-trò-người-dùng)
2. [Catalog âm nhạc](#2-catalog-âm-nhạc)
3. [Sản phẩm & Bán hàng](#3-sản-phẩm--bán-hàng)
4. [Quy trình đơn hàng](#4-quy-trình-đơn-hàng)
5. [Thanh toán](#5-thanh-toán)
6. [Thông báo & Sự kiện nghiệp vụ](#6-thông-báo--sự-kiện-nghiệp-vụ)

## 0. Tổng quan tính năng

### 0.1 Xác thực & Tài khoản
* Đăng ký tài khoản (Local)
* Đăng nhập / đăng xuất (Local + Google Auth)
* Phân quyền theo vai trò (Guest / Customer / Admin)

### 0.2 Catalog & Sản phẩm
* Xem sản phẩm: general, random
* Lọc sản phẩm (thể loại nhạc, định dạng (cd, cassette, vinyl), nghệ sĩ, quốc gia, giá, thập niên)
* Tìm kiếm sản phẩm (tên album, single, ep, tên bài hát, thể loại nhạc, định dạng (cd, cassette, vinyl), nghệ sĩ, quốc gia)
* Xem chi tiết sản phẩm (thông tin nghệ sĩ, phiên bản, tracklist)
* Xem sản phẩm theo bộ sưu tập chủ đề (Curated Collections)
* Quản lý catalog (Admin: thêm / sửa / xóa nghệ sĩ, products)

### 0.3 Mua hàng
* Thêm sản phẩm vào giỏ hàng
* Đặt hàng
* Chọn phương thức thanh toán (Stripe)
* Đặt hàng trước (Pre-order)
* Theo dõi trạng thái đơn hàng
* Hủy đơn hàng (Customer: chỉ khi Pending / Admin: từ Confirmed trở đi)
* Đánh giá sản phẩm sau khi đơn hoàn thành

### 0.4 Thanh toán
* Thanh toán online qua cổng Stripe
* Ghi nhận lịch sử giao dịch

### 0.5 Quản lý đơn hàng (Admin)
* Xác nhận đơn hàng
* Cập nhật trạng thái ship
* Hủy đơn hàng
* Xem báo cáo doanh thu



### 0.6 Thông báo
* Gửi email xác nhận khi đơn được tạo
* Gửi email khi đơn được xác nhận
* Gửi email kèm thông tin vận chuyển khi đơn được ship
* Gửi email mời đánh giá khi đơn hoàn thành
* Gửi email thông báo khi đơn bị hủy

---

## 1. Vai trò người dùng

| Vai trò | Mô tả | Quyền hạn |
|---|---|---|
| **Guest** | Khách vãng lai, chưa đăng nhập | Xem danh mục, tìm kiếm & lọc sản phẩm |
| **Customer** | Khách hàng đã đăng ký (Email hoặc Google) | Mua hàng, theo dõi đơn |
| **Admin** | Chủ shop | Quản lý catalog, xác nhận / ship / hủy đơn, xem báo cáo doanh thu, xem log hành động |

---

## 2. Catalog âm nhạc

Kho dữ liệu âm nhạc — tách biệt hoàn toàn khỏi logic mua bán. Đây là nền tảng để AI hoạt động và để quản lý thông tin sản phẩm chính xác.

### 2.1 Phân cấp dữ liệu

```
Nghệ sĩ  ──→  Bản phát hành gốc  ──→  Phiên bản cụ thể  ──→  Tracklist
```

| Thực thể | Thông tin lưu trữ | Ví dụ |
|---|---|---|
| **Nghệ sĩ** | Tên, tiểu sử, thể loại, quốc gia | Pink Floyd, Miles Davis |
| **Bản phát hành gốc** | Tên album, năm phát hành, thể loại, ảnh bìa | "Dark Side of the Moon" (1973) |
| **Phiên bản cụ thể** | Lần ép, quốc gia, định dạng, số catalog, hãng đĩa | US first press, Japan obi 1976, 2011 Remaster |
| **Tracklist** | Vị trí, tên bài, thời lượng | Side A — Track 1: Speak to Me |

### 2.2 Lý do phân tách Bản gốc và Phiên bản

Một album gốc có thể có hàng chục phiên bản khác nhau, mỗi phiên bản có giá, tình trạng và tồn kho riêng biệt. Phân cấp này giúp quản lý chính xác từng bản in mà không trùng lặp thông tin nghệ thuật.

### 2.3 Hãng đĩa (Label)

Lưu trữ thông tin hãng đĩa liên kết với từng phiên bản cụ thể: tên hãng, quốc gia, năm thành lập, website.

---

## 3. Sản phẩm & Bán hàng

### 3.1 Loại sản phẩm

- Vinyl
- CD
- Cassette

### 3.2 Biến thể sản phẩm

Mỗi sản phẩm có thể có nhiều biến thể. Mỗi biến thể có giá và tồn kho độc lập.

**Vinyl**

| Nhóm | Ví dụ |
|---|---|
| Màu đĩa | Black, Colored, Splatter, Picture disc |
| Trọng lượng | 140g (standard), 180g (audiophile) |
| Tốc độ | 33⅓ RPM, 45 RPM |
| Số đĩa | 1LP, 2LP, Box set |
| Phiên bản bìa | Standard sleeve, Gatefold, Obi strip |
| Chữ ký | Signed by artist |

**CD**

| Nhóm | Ví dụ |
|---|---|
| Nội dung | Standard, Deluxe / Expanded (bonus tracks), Box set |
| Phiên bản quốc gia | Japan edition (thường có bonus track riêng) |
| Chữ ký | Signed by artist |

**Cassette**

| Nhóm | Ví dụ |
|---|---|
| Màu băng | Black, Clear, White, Colored |
| Phiên bản | Standard, Limited edition |
| Chữ ký | Signed by artist |

### 3.3 Tính năng đặc biệt

**Limited Edition**
- Số lượng giới hạn được xác định từ trước.
- Không được phép tăng số lượng giới hạn sau khi bắt đầu bán.

**Pre-order**
- Khách hàng đặt trước trước ngày phát hành.
- Tồn kho không bị trừ cho đến khi đến ngày phát hành thực tế.

### 3.4 Quy tắc tồn kho

- Hết hàng → tự động đánh dấu không thể mua, ẩn khỏi danh sách có hàng.
- Không được xóa sản phẩm khi đang có đơn hàng ở trạng thái Pending hoặc Confirmed.
- Khi hàng về kho → hệ thống tự động thông báo cho các khách hàng có sản phẩm đó trong Wantlist.

### 3.5 Bộ sưu tập sản phẩm (Curated Collections)

Admin có thể tạo các bộ sưu tập theo chủ đề biên tập.

Ví dụ: *Horror Soundtracks*, *Video Game OST*, *Vietnam New Wave*.

---

## 4. Quy trình đơn hàng

### 4.1 Vòng đời đơn hàng

```
Pending → Confirmed → Shipped → Delivered → Completed
             ↓
          Cancelled (chỉ Admin từ trạng thái này trở đi)
```

| Trạng thái | Mô tả |
|---|---|
| **Pending** | Khách vừa đặt hàng, chờ xác nhận |
| **Confirmed** | Admin đã xác nhận đơn |
| **Shipped** | Đơn đã được gửi đi |
| **Delivered** | Đơn đã đến tay khách |
| **Completed** | Hoàn tất, khách có thể để lại đánh giá |
| **Cancelled** | Đơn bị hủy |

### 4.2 Quy tắc nghiệp vụ

- **Checkout:** trừ tồn kho ngay khi đặt hàng thành công.
- **Hủy ở trạng thái Pending:** hoàn toàn bộ tồn kho về như cũ.
- **Hủy từ Confirmed trở lên:** chỉ Admin mới có quyền thực hiện.
- **Mỗi đơn chỉ có đúng 1 lần thanh toán.**


---

## 5. Thanh toán

### 5.1 Phương thức thanh toán

| Phương thức | Mô tả |
|---|---|
| **Stripe** | Thanh toán online qua cổng Stripe |

### 5.2 Thông tin ghi nhận

Mỗi lần thanh toán lưu: số tiền, phương thức thanh toán, mã giao dịch, thời điểm thanh toán, trạng thái.

---

## 6. Thông báo & Sự kiện nghiệp vụ

### 6.1 Thông báo theo vòng đời đơn hàng

| Sự kiện | Hành động thông báo |
|---|---|
| Đơn được tạo | Gửi email xác nhận đơn hàng cho khách |
| Đơn được xác nhận | Gửi email cập nhật trạng thái |
| Đơn đã ship | Gửi email kèm thông tin vận chuyển |
| Đơn hoàn thành | Gửi email mời khách đánh giá sản phẩm |
| Đơn bị hủy | Gửi email thông báo hủy đơn |


---

