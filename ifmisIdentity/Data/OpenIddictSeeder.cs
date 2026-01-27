using OpenIddict.Core;
using ifmisIdentity.Models; // for MyApplication, etc.

namespace ifmisIdentity.Data
{
    public static class OpenIddictSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            // Use the custom MyApplication type
            var manager = scope.ServiceProvider
                .GetRequiredService<OpenIddictApplicationManager<MyApplication>>();

            var existing = await manager.FindByClientIdAsync("my_client_id");
            if (existing == null)
            {
                await manager.CreateAsync(new OpenIddict.Abstractions.OpenIddictApplicationDescriptor
                {
                    ClientId = "my_client_id",
                    ClientSecret = "my_client_secret",
                    DisplayName = "My Client Credentials Client",
                    Permissions =
                    {
                        OpenIddict.Abstractions.OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddict.Abstractions.OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddict.Abstractions.OpenIddictConstants.Permissions.GrantTypes.Password

                    }
                });
            }
        }
    }
}
