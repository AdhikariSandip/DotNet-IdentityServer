using ifmisIdentity.Configuration;
using ifmisIdentity.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Simple console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.ConfigureIdentity(builder.Configuration);
builder.Services.ConfigureOpenIddict(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
              "https://localhost:5080",
              "http://localhost:4500"
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1",
        Description = "API documentation for the Identity Service",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "Binetsupport@example.com"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// CRITICAL: Enable detailed errors for troubleshooting
app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Microservice API V1");
    options.RoutePrefix = string.Empty;
});

// Database seeding with better error handling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seedLogger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        seedLogger.LogInformation("Starting database seeding...");
        await IdentitySeeder.SeedAsync(services, seedLogger);
        await OpenIddictSeeder.SeedAsync(services);
        seedLogger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        seedLogger.LogError(ex, "An error occurred while seeding the database.");
        // Don't throw - allow app to continue
    }
}

app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();