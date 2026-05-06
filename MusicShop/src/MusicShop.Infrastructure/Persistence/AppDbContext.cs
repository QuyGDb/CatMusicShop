using Microsoft.EntityFrameworkCore;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Entities.System;
using MusicShop.Domain.Entities.Messaging;
using System.Reflection;

namespace MusicShop.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    // Table mapping according to ERD numbering

    // 1. User Roles (System)
    public DbSet<User> Users => Set<User>();

    // 2. Music Catalog
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Release> Releases => Set<Release>(); // Master
    public DbSet<ReleaseVersion> ReleaseVersions => Set<ReleaseVersion>(); // Pressing
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<ArtistGenre> ArtistGenres => Set<ArtistGenre>();
    public DbSet<ReleaseGenre> ReleaseGenres => Set<ReleaseGenre>();

    // 3. Products & Sales (Shop)
    public DbSet<Product> Products => Set<Product>();
    public DbSet<VinylAttributes> VinylAttributes => Set<VinylAttributes>();
    public DbSet<CdAttributes> CdAttributes => Set<CdAttributes>();
    public DbSet<CassetteAttributes> CassetteAttributes => Set<CassetteAttributes>();
    public DbSet<CuratedCollection> CuratedCollections => Set<CuratedCollection>();
    public DbSet<CuratedCollectionItem> CuratedCollectionItems => Set<CuratedCollectionItem>();

    // 4. Order Process (Orders)
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // 5. Payments
    public DbSet<Payment> Payments => Set<Payment>();

    // Legacy/Extra
    public DbSet<AdminActivityLog> AdminActivityLogs => Set<AdminActivityLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // 8. Messaging (Outbox/Inbox)
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
