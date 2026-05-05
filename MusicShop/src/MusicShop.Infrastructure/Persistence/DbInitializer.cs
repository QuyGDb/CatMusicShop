using Microsoft.EntityFrameworkCore;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Entities.System;
using MusicShop.Domain.Enums;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Entities.Shop;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Reflection;
using MusicShop.Infrastructure.Security;


namespace MusicShop.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher, AdminSettings adminSettings)
    {
        await context.Database.MigrateAsync();

        // 1. Seed Admin
        if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            User adminUser = new User
            {
                Email = adminSettings.Email,
                FullName = adminSettings.FullName,
                PasswordHash = passwordHasher.Hash(adminSettings.Password),
                Role = UserRole.Admin,
                IdentityProvider = "Local"
            };


            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }

        // 2. Seed Genres
        if (!await context.Genres.AnyAsync())
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MusicShop.Infrastructure.Persistence.SeedData.genres.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            var records = csv.GetRecords<dynamic>();
            List<Genre> genres = new();

            foreach (var record in records)
            {
                genres.Add(new Genre
                {
                    Name = record.Name,
                    Slug = record.Slug
                });
            }

            await context.Genres.AddRangeAsync(genres);
            await context.SaveChangesAsync();
        }

        // 3. Seed Artists
        if (!await context.Artists.AnyAsync())
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MusicShop.Infrastructure.Persistence.SeedData.artists.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<dynamic>();
            var genresMap = await context.Genres.ToDictionaryAsync(g => g.Name);

            List<Artist> artists = new();

            foreach (var record in records)
            {
                var artist = new Artist
                {
                    Name = record.Name,
                    Bio = record.Bio,
                    Country = record.Country,
                    Slug = Slugify(record.Name),
                    ImageUrl = record.ImageUrl
                };

                // Link genres via navigation property
                string genresStr = record.Genres ?? string.Empty;
                var artistGenreNames = genresStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(g => g.Trim());

                foreach (var genreName in artistGenreNames)
                {
                    if (genresMap.TryGetValue(genreName, out var genre))
                    {
                        artist.ArtistGenres.Add(new ArtistGenre
                        {
                            Genre = genre
                        });
                    }
                }

                artists.Add(artist);
            }

            await context.Artists.AddRangeAsync(artists);
        }

        await context.SaveChangesAsync();

        // 4. Seed Labels
        if (!await context.Labels.AnyAsync())
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MusicShop.Infrastructure.Persistence.SeedData.labels.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<dynamic>();
            List<Label> labels = new();

            foreach (var record in records)
            {
                labels.Add(new Label
                {
                    Name = record.Name,
                    Slug = record.Slug,
                    Country = record.Country,
                    FoundedYear = int.TryParse(record.FoundedYear?.ToString(), out int year) ? year : null,
                    Website = record.Website
                });
            }

            await context.Labels.AddRangeAsync(labels);
        }

        await context.SaveChangesAsync();

        // 5. Seed Releases
        if (!await context.Releases.AnyAsync())
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.releases.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            List<dynamic> records = csv.GetRecords<dynamic>().ToList();
            Dictionary<string, Artist> artistsMap = await context.Artists.ToDictionaryAsync(a => a.Name);
            Dictionary<string, Genre> genresMap = await context.Genres.ToDictionaryAsync(g => g.Name);

            List<Release> releases = new();

            foreach (var record in records)
            {
                string artistName = record.ArtistName ?? string.Empty;
                if (!artistsMap.TryGetValue(artistName, out Artist? artist)) continue;

                Release release = new Release
                {
                    Title = record.Title,
                    Slug = record.Slug,
                    Year = int.TryParse(record.Year?.ToString(), out int year) ? year : 0,
                    Description = record.Description,
                    Artist = artist,
                    CoverUrl = record.ImageUrl
                };

                // Link genres via navigation property
                string genresStr = record.Genres ?? string.Empty;
                IEnumerable<string> releaseGenreNames = genresStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(g => g.Trim());

                foreach (var genreName in releaseGenreNames)
                {
                    if (genresMap.TryGetValue(genreName, out Genre? genre))
                    {
                        release.ReleaseGenres.Add(new ReleaseGenre
                        {
                            Genre = genre
                        });
                    }
                }

                releases.Add(release);
            }

            await context.Releases.AddRangeAsync(releases);
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {releases.Count} releases.");
        }

        // 6. Seed Tracks
        if (!await context.Tracks.AnyAsync())
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.tracks.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            List<dynamic> records = csv.GetRecords<dynamic>().ToList();
            Dictionary<string, Release> releasesMap = await context.Releases.ToDictionaryAsync(release => release.Title);

            List<Track> tracks = new();

            foreach (dynamic record in records)
            {
                string releaseTitle = record.ReleaseTitle ?? string.Empty;
                if (!releasesMap.TryGetValue(releaseTitle, out Release? release)) continue;

                tracks.Add(new Track
                {
                    Release = release,
                    Position = int.TryParse(record.Position?.ToString(), out int position) ? position : 0,
                    Title = record.Title ?? "Unknown Track",
                    Side = record.Side,
                    DurationSeconds = int.TryParse(record.DurationSeconds?.ToString(), out int seconds) ? seconds : null
                });
            }

            await context.Tracks.AddRangeAsync(tracks);
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {tracks.Count} tracks.");
        }

        // 7. Seed ReleaseVersions
        if (!await context.ReleaseVersions.AnyAsync())
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.release_versions.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            List<dynamic> records = csv.GetRecords<dynamic>().ToList();
            Dictionary<string, Release> releasesMap = await context.Releases.ToDictionaryAsync(r => r.Title);
            Dictionary<string, Label> labelsMap = await context.Labels.ToDictionaryAsync(l => l.Name);

            List<ReleaseVersion> versions = new();

            foreach (var record in records)
            {
                string releaseTitle = record.ReleaseTitle ?? string.Empty;
                string labelName = record.LabelName ?? string.Empty;

                if (!releasesMap.TryGetValue(releaseTitle, out Release? release)) continue;

                // Auto-create unknown labels instead of skipping — Discogs returns many label names
                // that don't exist in the manually curated labels.csv
                if (!labelsMap.TryGetValue(labelName, out Label? label))
                {
                    label = new Label
                    {
                        Name = labelName,
                        Slug = labelName.ToLowerInvariant()
                            .Replace(" ", "-")
                            .Replace(".", "")
                            .Replace("'", "")
                            .Replace("(", "")
                            .Replace(")", "")
                            .Replace("&", "and")
                            .Trim('-')
                    };
                    await context.Labels.AddAsync(label);
                    await context.SaveChangesAsync();
                    labelsMap[labelName] = label;
                }

                versions.Add(new ReleaseVersion
                {
                    Name = record.VersionName,
                    Release = release,
                    Label = label,
                    Format = Enum.TryParse<ReleaseFormat>(record.Format?.ToString(), out ReleaseFormat format) ? format : ReleaseFormat.Vinyl,
                    PressingCountry = record.PressingCountry,
                    PressingYear = int.TryParse(record.PressingYear?.ToString(), out int year) ? year : null,
                    CatalogNumber = record.CatalogNumber,
                    Notes = record.Notes
                });
            }

            await context.ReleaseVersions.AddRangeAsync(versions);
        }

        await context.SaveChangesAsync();

        // 8. Seed Products
        if (!await context.Products.AnyAsync())
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.product.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            List<dynamic> records = csv.GetRecords<dynamic>().ToList();
            List<ReleaseVersion> versions = await context.ReleaseVersions
                .Include(v => v.Release)
                .ToListAsync();
            
            Dictionary<string, ReleaseVersion> versionsMap = versions.ToDictionary(v => $"{v.Release.Title}|{v.Name}");

            List<Product> products = new();

            foreach (var record in records)
            {
                string releaseTitle = record.ReleaseTitle?.ToString() ?? string.Empty;
                string versionName = record.VersionName?.ToString() ?? string.Empty;
                string key = $"{releaseTitle}|{versionName}";

                if (!versionsMap.TryGetValue(key, out ReleaseVersion? version))
                {
                    Console.WriteLine($"[Seed] Warning: ReleaseVersion not found for key: {key}. Skipping product.");
                    continue;
                }

                string name = record.DisplayName?.ToString() ?? $"{releaseTitle} - {versionName}";
                products.Add(new Product
                {
                    ReleaseVersionId = version.Id,
                    Name = name,
                    Slug = Slugify(name),
                    Price = decimal.TryParse(record.Price?.ToString(), out decimal price) ? price : 0,
                    StockQty = int.TryParse(record.StockQty?.ToString(), out int stock) ? stock : 0,
                    IsAvailable = true,
                    IsActive = true,
                    IsLimited = bool.TryParse(record.IsLimited?.ToString(), out bool limited) && limited,
                    LimitedQty = int.TryParse(record.LimitedQty?.ToString(), out int lQty) ? lQty : null,
                    IsPreorder = bool.TryParse(record.IsPreorder?.ToString(), out bool preorder) && preorder,
                    PreorderReleaseDate = DateTime.TryParse(record.PreorderReleaseDate?.ToString(), out DateTime pDate) ? pDate : null,
                    IsSigned = bool.TryParse(record.IsSigned?.ToString(), out bool signed) && signed,
                    CoverUrl = record.ImageUrl
                });
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {products.Count} products.");
        }

        // 9. Seed CuratedCollections
        if (!await context.CuratedCollections.AnyAsync())
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MusicShop.Infrastructure.Persistence.SeedData.curateCollection.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<dynamic>();
            List<CuratedCollection> collections = new();

            foreach (var record in records)
            {
                collections.Add(new CuratedCollection
                {
                    Title = record.Title,
                    Description = record.Description,
                    IsPublished = bool.TryParse(record.IsPublished?.ToString(), out bool published) && published
                });
            }

            await context.CuratedCollections.AddRangeAsync(collections);
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {collections.Count} curated collections.");
        }

        // 10. Seed CuratedCollectionItems
        if (!await context.Set<CuratedCollectionItem>().AnyAsync())
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MusicShop.Infrastructure.Persistence.SeedData.curateCollectionItem.csv";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<dynamic>();
            var collectionsMap = await context.CuratedCollections.ToDictionaryAsync(c => c.Title);
            var productsMap = await context.Products.ToDictionaryAsync(p => p.Name);

            List<CuratedCollectionItem> items = new();

            foreach (var record in records)
            {
                string collectionTitle = record.CollectionTitle?.ToString() ?? string.Empty;
                string productName = record.ProductName?.ToString() ?? string.Empty;

                if (collectionsMap.TryGetValue(collectionTitle, out var collection) &&
                    productsMap.TryGetValue(productName, out var product))
                {
                    items.Add(new CuratedCollectionItem
                    {
                        CollectionId = collection.Id,
                        ProductId = product.Id,
                        SortOrder = int.TryParse(record.SortOrder?.ToString(), out int order) ? order : 0
                    });
                }
                else
                {
                    Console.WriteLine($"[Seed] Warning: Collection '{collectionTitle}' or Product '{productName}' not found. Skipping item.");
                }
            }

            await context.Set<CuratedCollectionItem>().AddRangeAsync(items);
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {items.Count} curated collection items.");
        }
    }

    private static string Slugify(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        return text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace("'", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("&", "and")
            .Replace(",", "")
            .Trim('-');
    }
}
