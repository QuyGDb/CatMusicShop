# CrawlData Scripts

Independent C# scripts for building Seed Data for the music shop. These scripts run directly using .NET 10 without requiring a project file.

## Execution Order (MUST be sequential)

```
STEP 1  DiscoverReleases.cs      — Find albums from artists.csv → write to releases.csv
STEP 2  EnrichRealData.cs        — Fetch Vinyl/CD/Cassette versions from Discogs → write to release_versions.csv + product.csv
STEP 3  DownloadTracks.cs        — Fetch tracklists from Discogs → write to tracks.csv
STEP 4  DownloadArtistImages.cs  — Download artist images → save locally
STEP 5  DownloadReleaseImages.cs — Download album cover images → save locally
STEP 6  DownloadProductImages.cs — Download physical product images (CD/Vinyl) → save locally
STEP 7  UploadArtistImages.cs    — Upload artist images to API → update artists.csv
STEP 8  UploadReleaseImages.cs   — Upload album images to API → update releases.csv
STEP 9  UploadProductImages.cs   — Upload product images to API → update product.csv
```

### Run from the CrawlData directory

```powershell
cd "d:\CatMusicShop\CrawlData"

dotnet DiscoverReleases.cs
dotnet EnrichRealData.cs
dotnet DownloadTracks.cs
dotnet DownloadArtistImages.cs
dotnet DownloadReleaseImages.cs
dotnet DownloadProductImages.cs

# Note: The API (MusicShop.API) must be running to perform the upload steps
dotnet UploadArtistImages.cs
dotnet UploadReleaseImages.cs
dotnet UploadProductImages.cs
```

> **Note**: These scripts are designed to be idempotent and can be run multiple times without duplicating data.  
> Each script depends on the output of the previous one — **do not change the order**.

---

## Data Flow

```
artists.csv
    ↓ [DiscoverReleases.cs — iTunes API]
releases.csv
    ↓ [EnrichRealData.cs — Discogs API]
release_versions.csv  +  product.csv
    ↓ [DownloadTracks.cs — Discogs API]
tracks.csv
    ↓ [DownloadArtistImages / DownloadReleaseImages / DownloadProductImages]
Local Images (SeedData/artists, releases, products)
    ↓ [UploadXXXImages.cs — MusicShop API]
Updated CSVs with ImageUrl (CloudFront)
```

---

## Script Descriptions

| Script | API | Input | Output |
|--------|-----|-------|--------|
| `DiscoverReleases.cs` | iTunes Search API | `artists.csv` | `releases.csv` |
| `EnrichRealData.cs` | Discogs API | `releases.csv` | `release_versions.csv`, `product.csv` |
| `DownloadTracks.cs` | Discogs API | `releases.csv` | `tracks.csv` |
| `DownloadArtistImages.cs` | Discogs API | `artists.csv` | `SeedData/artists/` |
| `DownloadReleaseImages.cs`| iTunes API | `releases.csv` | `SeedData/releases/` |
| `DownloadProductImages.cs`| Discogs API | `product.csv` | `SeedData/products/` |
| `UploadArtistImages.cs` | MusicShop API | `artists.csv` | `artists.csv` (ImageUrl) |
| `UploadReleaseImages.cs`| MusicShop API | `releases.csv` | `releases.csv` (ImageUrl) |
| `UploadProductImages.cs` | MusicShop API | `product.csv` | `product.csv` (ImageUrl) |
