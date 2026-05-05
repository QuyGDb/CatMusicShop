using MusicShop.Infrastructure;
using System.Text.Json.Serialization;
using MusicShop.Application;
using Serilog;
using MusicShop.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MusicShop.API.Middleware;
using MusicShop.Domain.Interfaces;
using MusicShop.Infrastructure.Persistence;
using MusicShop.Infrastructure.Messaging;
using Hangfire;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 1. Setup Logging (Serilog)
builder.Host.UseSerilog((context, logger) =>
    logger.WriteTo.Console()
          .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
          .ReadFrom.Configuration(context.Configuration));

// 2. Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://192.168.1.5:3000",
                "https://catmusicshop.duckdns.org"
            )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "MusicShop API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"Please enter token in the following format: Bearer {token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Register selective Swagger lock filter
    c.OperationFilter<MusicShop.API.Infrastructure.AuthorizeCheckOperationFilter>();
});

// 3. Register Clean Architecture Layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Configure JWT Authentication
JwtSettings jwtSettings = new();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseExceptionHandler();
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("FrontendPolicy");

// HTTP Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    await next();
});

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 6. Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MusicShop.API.Infrastructure.HangfireAdminAuthorizationFilter() }
});

// 7. Recurring Jobs
using (IServiceScope scope = app.Services.CreateScope())
{
    IRecurringJobManager recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Remove old/obsolete jobs from database
    recurringJobs.RemoveIfExists("process-outbox-messages");
    recurringJobs.RemoveIfExists("process-inbox-messages");

    recurringJobs.AddOrUpdate<MessagePollingJob>(
        "message-poller",
        job => job.PollAsync(default),
        "*/2 * * * *"); // Every 2 minutes
}


//5.Initialize Database & Seed data
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    try
    {
        AppDbContext context = services.GetRequiredService<AppDbContext>();
        IPasswordHasher passwordHasher = services.GetRequiredService<IPasswordHasher>();
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
        
        AdminSettings adminSettings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AdminSettings>>().Value;


        logger.LogInformation("Checking database for seeding...");
        await DbInitializer.SeedAsync(context, passwordHasher, adminSettings);


        logger.LogInformation("Database initialization complete.");
    }
    catch (Exception ex)
    {
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization/seeding.");
    }
}

app.Run();

public partial class Program { }
