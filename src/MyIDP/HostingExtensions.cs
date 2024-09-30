// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer;
using Microsoft.IdentityModel.Tokens;
using Rsk.Saml.Configuration;
using Serilog;
using Duende.IdentityServer.Configuration;
using Rsk.Saml.Generators;
namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer(options =>
            {
                options.KeyManagement.Enabled = true;
                options.EmitStaticAudienceClaim=true;

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
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryClients(Config.Clients)
            .AddTestUsers(TestUsers.Users)
            .AddSamlPlugin(options =>
            {
                options.Licensee = "Demo";
                options.LicenseKey = "eyJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjQtMTAtMTlUMDA6MDA6MDAiLCJpYXQiOiIyMDI0LTA5LTE5VDA5OjE2OjA3Iiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.Px+W0ZpuAVL+jeIdppLYDcP6jrhBD7Llkv/56MMIcZ9D/VPZqZhhQx/wqvCfuGxI8QMg5/z0SXIn1IWPKeFcWdBYVyoixwzWbsi8or9g60rcePyyCn7ucPtu90A5mPs/mf6mQIc7qasFiL1RC7Dx0r153s3Im5WTOqr7O8gK45bNOqEninJ89pDxyX7p7I6a4BuSRxqDJYes9iP1NpL20YZbSrABW4cegAl7MsZmLc1zaEwYhN4+a3Yhap9JwKTUI3twQcOmKmPf1E7xeS5QunxIA4AvvicXuRjGMHnaUfzynEeTv9guiVh42bbbTxSLpVle7b6SeDcUrOBLAHzJHctLrio3zWYKdilnnmtDKlr8BMpmk9mH6pXsyC7fbG4wS0BbiAAday+PYjV/jOxdn8IK3xTrecp1+2w0D0obTLBTuVi1bWI5jeyHXXj9nBTAHnV01t9/X3jUSBDNHuRAltgBiAwuetRoRwXM4xSIdQS8UoXnFwoLNtgx5lUAaNdaNmnrDsA9jszZjDXgTZT4noQilbcC5wf+IhdgGyXMVSsOvhlS74Kcm1XBFhIzptQuF4QW+fy+qGLMZNr+1qUcJ/uTY2CRYMjcH2sHN9AJDWg89+UDTpm/Y+WhFAPsomYTXnpTjzDVzV1QULXDcL2lDQXfYsqytCrtvK+W5WikUMk=";
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

//builder.Services.AddTransient<ISamlNameIdGenerator, CustomNameIdGenerator>();
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
