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
    public static async Task SeedAsync(
        IRepository<User> userRepository,
        IRepository<Genre> genreRepository,
        IRepository<Artist> artistRepository,
        IRepository<Label> labelRepository,
        IRepository<Release> releaseRepository,
        IRepository<Track> trackRepository,
        IRepository<ReleaseVersion> releaseVersionRepository,
        IRepository<Product> productRepository,
        IRepository<CuratedCollection> curatedCollectionRepository,
        IRepository<CuratedCollectionItem> curatedCollectionItemRepository,
        AppDbContext context,
        IPasswordHasher passwordHasher,
        AdminSettings adminSettings)
    {
        await context.Database.MigrateAsync();

        // 1. Seed Admin
        if (!await userRepository.AnyAsync(user => user.Role == UserRole.Admin))
        {
            User adminUser = new User
            {
                Email = adminSettings.Email,
                FullName = adminSettings.FullName,
                PasswordHash = passwordHasher.Hash(adminSettings.Password),
                Role = UserRole.Admin,
                IdentityProvider = "Local"
            };

            userRepository.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // 2. Seed Genres
        if (!await genreRepository.AnyAsync(genre => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.genres.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            IEnumerable<dynamic> records = csv.GetRecords<dynamic>();
            List<Genre> genres = new List<Genre>();

            foreach (dynamic record in records)
            {
                genres.Add(new Genre
                {
                    Name = record.Name,
                    Slug = record.Slug
                });
            }

            foreach (Genre genre in genres)
            {
                genreRepository.Add(genre);
            }
            await context.SaveChangesAsync();
        }

        // 3. Seed Artists
        if (!await artistRepository.AnyAsync(artist => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.artists.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            IEnumerable<dynamic> records = csv.GetRecords<dynamic>();
            Dictionary<string, Genre> genresMap = await context.Genres.ToDictionaryAsync(genre => genre.Name);

            List<Artist> artists = new List<Artist>();

            foreach (dynamic record in records)
            {
                Artist artist = new Artist
                {
                    Name = record.Name,
                    Bio = record.Bio,
                    Country = record.Country,
                    Slug = Slugify(record.Name),
                    ImageUrl = record.ImageUrl
                };

                // Link genres via navigation property
                string genresStr = record.Genres ?? string.Empty;
                IEnumerable<string> artistGenreNames = genresStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(genreName => genreName.Trim());

                foreach (string genreName in artistGenreNames)
                {
                    if (genresMap.TryGetValue(genreName, out Genre? genre))
                    {
                        artist.ArtistGenres.Add(new ArtistGenre
                        {
                            Genre = genre
                        });
                    }
                }

                artists.Add(artist);
            }

            foreach (Artist artist in artists)
            {
                artistRepository.Add(artist);
            }
        }

        await context.SaveChangesAsync();

        // 4. Seed Labels
        if (!await labelRepository.AnyAsync(label => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.labels.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            IEnumerable<dynamic> records = csv.GetRecords<dynamic>();
            List<Label> labels = new List<Label>();

            foreach (dynamic record in records)
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

            foreach (Label label in labels)
            {
                labelRepository.Add(label);
            }
        }

        await context.SaveChangesAsync();

        // 5. Seed Releases
        if (!await releaseRepository.AnyAsync(release => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.releases.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            List<dynamic> records = csv.GetRecords<dynamic>().ToList();
            Dictionary<string, Artist> artistsMap = await context.Artists.ToDictionaryAsync(artist => artist.Name);
            Dictionary<string, Genre> genresMap = await context.Genres.ToDictionaryAsync(genre => genre.Name);

            List<Release> releases = new List<Release>();

            foreach (dynamic record in records)
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
                                                 .Select(genreName => genreName.Trim());

                foreach (string genreName in releaseGenreNames)
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

            foreach (Release release in releases)
            {
                releaseRepository.Add(release);
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {releases.Count} releases.");
        }

        // 6. Seed Tracks
        if (!await trackRepository.AnyAsync(track => true))
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

            List<Track> tracks = new List<Track>();

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

            foreach (Track track in tracks)
            {
                trackRepository.Add(track);
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {tracks.Count} tracks.");
        }

        // 7. Seed ReleaseVersions
        if (!await releaseVersionRepository.AnyAsync(version => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.release_versions.csv";

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
            Dictionary<string, Label> labelsMap = await context.Labels.ToDictionaryAsync(label => label.Name);

            List<ReleaseVersion> versions = new List<ReleaseVersion>();

            foreach (dynamic record in records)
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
                    labelRepository.Add(label);
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

            foreach (ReleaseVersion version in versions)
            {
                releaseVersionRepository.Add(version);
            }
        }

        await context.SaveChangesAsync();

        // 8. Seed Products
        if (!await productRepository.AnyAsync(product => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.product.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            List<dynamic> records = csv.GetRecords<dynamic>().ToList();
            List<ReleaseVersion> versions = await context.ReleaseVersions
                .Include(version => version.Release)
                .ToListAsync();
            
            Dictionary<string, ReleaseVersion> versionsMap = versions.ToDictionary(version => $"{version.Release.Title}|{version.Name}");

            List<Product> products = new List<Product>();

            foreach (dynamic record in records)
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

            foreach (Product product in products)
            {
                productRepository.Add(product);
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {products.Count} products.");
        }

        // 9. Seed CuratedCollections
        if (!await curatedCollectionRepository.AnyAsync(collection => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.curateCollection.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            IEnumerable<dynamic> records = csv.GetRecords<dynamic>();
            List<CuratedCollection> collections = new List<CuratedCollection>();

            foreach (dynamic record in records)
            {
                collections.Add(new CuratedCollection
                {
                    Title = record.Title,
                    Description = record.Description,
                    IsPublished = bool.TryParse(record.IsPublished?.ToString(), out bool published) && published
                });
            }

            foreach (CuratedCollection collection in collections)
            {
                curatedCollectionRepository.Add(collection);
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] Successfully seeded {collections.Count} curated collections.");
        }

        // 10. Seed CuratedCollectionItems
        if (!await curatedCollectionItemRepository.AnyAsync(item => true))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MusicShop.Infrastructure.Persistence.SeedData.curateCollectionItem.csv";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new FileNotFoundException("Seed data file not found", resourceName);

            using StreamReader reader = new StreamReader(stream);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            IEnumerable<dynamic> records = csv.GetRecords<dynamic>();
            Dictionary<string, CuratedCollection> collectionsMap = await context.CuratedCollections.ToDictionaryAsync(collection => collection.Title);
            Dictionary<string, Product> productsMap = await context.Products.ToDictionaryAsync(product => product.Name);

            List<CuratedCollectionItem> items = new List<CuratedCollectionItem>();

            foreach (dynamic record in records)
            {
                string collectionTitle = record.CollectionTitle?.ToString() ?? string.Empty;
                string productName = record.ProductName?.ToString() ?? string.Empty;

                if (collectionsMap.TryGetValue(collectionTitle, out CuratedCollection? collection) &&
                    productsMap.TryGetValue(productName, out Product? product))
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

            foreach (CuratedCollectionItem item in items)
            {
                curatedCollectionItemRepository.Add(item);
            }
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
