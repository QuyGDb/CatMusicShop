# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["MusicShop.API/MusicShop.API.csproj", "MusicShop.API/"]
COPY ["MusicShop.Application/MusicShop.Application.csproj", "MusicShop.Application/"]
COPY ["MusicShop.Domain/MusicShop.Domain.csproj", "MusicShop.Domain/"]
COPY ["MusicShop.Infrastructure/MusicShop.Infrastructure.csproj", "MusicShop.Infrastructure/"]

RUN dotnet restore "MusicShop.API/MusicShop.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MusicShop.API"
RUN dotnet build "MusicShop.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MusicShop.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "MusicShop.API.dll"]
