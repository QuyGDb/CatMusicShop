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
    public DbSet<User> Users { get; set; }

    // 2. Music Catalog
    public DbSet<Artist> Artists { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Release> Releases { get; set; } // Master
    public DbSet<ReleaseVersion> ReleaseVersions { get; set; } // Pressing
    public DbSet<Label> Labels { get; set; }
    public DbSet<Track> Tracks { get; set; }

    // 3. Products & Sales (Shop)
    public DbSet<Product> Products { get; set; }
    public DbSet<CuratedCollection> CuratedCollections { get; set; }


    // 8. Messaging (Outbox/Inbox)
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
