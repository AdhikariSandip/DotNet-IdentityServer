using ifmisIdentity.Data;
using ifmisIdentity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

public static class OpenIddictConfig
{
    public static IServiceCollection ConfigureOpenIddict(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OpenIddictDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging(); // Remove in production after debugging
        });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<OpenIddictDbContext>()
                       .ReplaceDefaultEntities<MyApplication, MyAuthorization, MyScope, MyToken, int>();
            })
            .AddServer(options =>
            {
                // Set the issuer from configuration
                var issuer = configuration["JwtSettings:Issuer"];
                if (!string.IsNullOrEmpty(issuer))
                {
                    try
                    {
                        options.SetIssuer(new Uri(issuer));
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Invalid issuer URL: {issuer}", ex);
                    }
                }

                // Configure endpoints
                options.SetTokenEndpointUris("/connect/token");

                // Configure flows
                options.AllowPasswordFlow();
                options.AllowClientCredentialsFlow();
                options.AcceptAnonymousClients();

                // Configure token lifetime
                options.SetAccessTokenLifetime(TimeSpan.FromDays(100));

                // Configure encryption and signing
                var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");

                try
                {
                    if (environment == "Development")
                    {
                        // Development: Use development certificates
                        options.AddDevelopmentEncryptionCertificate();
                        options.AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        // Production: Try to load certificate, fallback to ephemeral
                        var certPath = Path.Combine(AppContext.BaseDirectory, "Asseet", "ForIdentity.pfx");

                        if (File.Exists(certPath))
                        {
                            var certificate = new X509Certificate2(certPath, "asdf1234",
                                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

                            options.AddEncryptionCertificate(certificate);
                            options.AddSigningCertificate(certificate);
                        }
                        else
                        {
                            // Fallback to ephemeral keys
                            Console.WriteLine($"WARNING: Certificate not found at {certPath}, using ephemeral keys");
                            options.AddEphemeralEncryptionKey();
                            options.AddEphemeralSigningKey();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to configure OpenIddict certificates", ex);
                }

                // Register claims and scopes
                options.RegisterClaims(new[] { "aud", "iss", "GlobalUserId", "M_1", "OrgUrl", "OrgName", "OrgId" });
                options.RegisterScopes(new[] { "openid", "profile", "email", "roles" });

                // Disable token encryption for debugging
                options.DisableAccessTokenEncryption();

                // Enable ASP.NET Core integration
                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough()
                       .DisableTransportSecurityRequirement(); // Only for development/testing
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}