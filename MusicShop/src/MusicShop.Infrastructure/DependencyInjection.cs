using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicShop.Domain.Interfaces;
using MusicShop.Infrastructure.Persistence;
using MusicShop.Infrastructure.Persistence.Repositories;
using MusicShop.Infrastructure.Security;
using MusicShop.Infrastructure.Services;
using MusicShop.Infrastructure.Payments;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Infrastructure.Storage;
using Amazon.S3;
using Amazon.Runtime;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using MusicShop.Application.Common.Models;
using MusicShop.Infrastructure.Messaging;

using Hangfire;
using Hangfire.PostgreSql;


namespace MusicShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configure DbContext with PostgreSQL
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            
            // Enable logging only in Development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.LogTo(Console.WriteLine, LogLevel.Information)
                       .EnableSensitiveDataLogging();
            }
        });

        // 2. Register Repository & UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IReleaseRepository, ReleaseRepository>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IReleaseVersionRepository, ReleaseVersionRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICuratedCollectionRepository, CuratedCollectionRepository>();

        // 3. Register Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // 4. Register JWT Services
        // ValidateOnStart: crash immediately at startup if config is missing/invalid
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateOnStart();

        services.AddOptions<GoogleSettings>()
            .Bind(configuration.GetSection(GoogleSettings.SectionName))
            .ValidateOnStart();

        services.AddOptions<AdminSettings>()
            .Bind(configuration.GetSection(AdminSettings.SectionName))
            .ValidateOnStart();


        services.AddStripeServices(configuration);

        services.AddScoped<ITokenService, JwtTokenService>();


        // 6. Register AWS S3
        services.AddOptions<S3Settings>()
            .Bind(configuration.GetSection(S3Settings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        AWSOptions awsOptions = configuration.GetAWSOptions("AWS");
        awsOptions.Credentials = new BasicAWSCredentials(
            configuration["AWS:AccessKey"],
            configuration["AWS:SecretKey"]);

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IImageService, S3ImageService>();
        
        // 7. Email Service
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateOnStart();

        services.AddScoped<IEmailService, GmailEmailService>();

        // 8. Configure Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer();

        // 9. Reliable Messaging
        services.AddScoped<IMessagePublisher, MediatRPublisher>();
        services.AddScoped<IInboxHandler, InboxHandler>();
        services.AddScoped<IMessageProcessor, MessageProcessor>();
        services.AddScoped<IJobService, HangfireJobService>();
        services.AddScoped<MessagePollingJob>();

        return services;
    }
}
