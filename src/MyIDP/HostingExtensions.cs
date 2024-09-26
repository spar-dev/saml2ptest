// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer;
using Microsoft.IdentityModel.Tokens;
using Rsk.Saml.Configuration;
using Serilog;
using Duende.IdentityServer.Configuration;
namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer(options =>
            {
                options.KeyManagement.Enabled = true;

                //Required to use the SAML plugin with Key Management
                options.KeyManagement.SigningAlgorithms = new[]
                {
                    new SigningAlgorithmOptions("RS256")
                    {
                        UseX509Certificate = true
                    }
                };
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddTestUsers(TestUsers.Users)
            .AddSamlPlugin(options =>
            {
                options.Licensee = "Demo";
                options.LicenseKey = "eyJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjQtMDktMjdUMDE6MDA6MDAuNDcyNTE4NyswMDowMCIsImlhdCI6IjIwMjQtMDgtMjhUMDE6MDA6MDAiLCJvcmciOiJERU1PIiwiYXVkIjoyfQ==.WT/bY3Jxc4T6OXnjGWNhhgWJiuGY+yVCEHi9hIwK4TZ42YN0Okg1S6/ChYlLXSAM4RWVUndVIHJwJao8vTfGAAMKDNV5rKyhL5MmAjGPrlohJ8f+i9PG+6sBMlZ0BCsQsjr/kbbkqOEOjZwvJqE0vjJxsC+qAjKneKzkoAcdycPJpgV/Q/2VxFbj/4tu73lOwFK0im3Q4pmCyqCundgYGFZMbLbydm3ywwtD49ePR9u6zhw82eW7iZmqLIX7PjZX+AWKdxTnWXymgXDIJ1wpnW6RDLRAXulI2ZEWcK/KdM7t8vcwq8AbIXXxC8bimGv+KIokZ/S5mKPCCM3R8/UF5hLBs2QubATVJaD0AXB4P/u+mDBpaYjXzukwufOm0xEP9GBfTyNdwagjYyY2quTXUuYjJNrO8rOtAK3C4hVv3RM43rnf24lldlo7c/uo5h5opmGdcN1D6STcxf1vtzkNNCnjmPejIYzLq02R0K+6AgmnT71rWzhFFo6OUz2Z3Qi6gk82pwB2ppWtvc/WrBDb+nJ1NK+j9OkWYj5Zg/A9CdfCh7hxrnOzxXoKixVWnNsGN9Yz4QbZz+SRtDWW0CbdCT0kvGne9NGrlv6Uhn6f9XF6JF73etK/MUWYdmo10eKHWch/39aih/qqttwDPQtAKizGsG7ReEoL7ZSSUZgqsDA=";
                //options.DefaultClaimMapping.Add("name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                options.DefaultClaimMapping.Add("SURID", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/SURID");
                 
                //Use Iterative SLO
                options.UseIFramesForSlo = false;
            })
            .AddInMemoryServiceProviders(Config.ServiceProvider);

        var authenticationBuilder = builder.Services.AddAuthentication();

        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        if(googleClientId != null && googleClientSecret != null)
        {
            authenticationBuilder.AddGoogle("Google", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });
        }
            
        authenticationBuilder.AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                options.SaveTokens = true;

                options.Authority = "https://demo.duendesoftware.com";
                options.ClientId = "interactive.confidential";
                options.ClientSecret = "secret";
                options.ResponseType = "code";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
            
       app.UseIdentityServer()
            .UseIdentityServerSamlPlugin();

        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
