using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Rsk.AspNetCore.Authentication.Saml2p;
using Rsk.Saml.Generators;
using WebClient;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "saml";
    })
    .AddCookie("Cookies", options =>
    {
        //Change cookie name to prevent collisions when running multiple applications on localhost
        options.Cookie.Name = "ServiceProviderSAML1";
    })    
    .AddSaml2p("saml", options =>
    {
          options.Licensee = "Demo";
        options.LicenseKey = "eyJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjQtMDktMjdUMDE6MDA6MDAuNDcyNTE4NyswMDowMCIsImlhdCI6IjIwMjQtMDgtMjhUMDE6MDA6MDAiLCJvcmciOiJERU1PIiwiYXVkIjoyfQ==.WT/bY3Jxc4T6OXnjGWNhhgWJiuGY+yVCEHi9hIwK4TZ42YN0Okg1S6/ChYlLXSAM4RWVUndVIHJwJao8vTfGAAMKDNV5rKyhL5MmAjGPrlohJ8f+i9PG+6sBMlZ0BCsQsjr/kbbkqOEOjZwvJqE0vjJxsC+qAjKneKzkoAcdycPJpgV/Q/2VxFbj/4tu73lOwFK0im3Q4pmCyqCundgYGFZMbLbydm3ywwtD49ePR9u6zhw82eW7iZmqLIX7PjZX+AWKdxTnWXymgXDIJ1wpnW6RDLRAXulI2ZEWcK/KdM7t8vcwq8AbIXXxC8bimGv+KIokZ/S5mKPCCM3R8/UF5hLBs2QubATVJaD0AXB4P/u+mDBpaYjXzukwufOm0xEP9GBfTyNdwagjYyY2quTXUuYjJNrO8rOtAK3C4hVv3RM43rnf24lldlo7c/uo5h5opmGdcN1D6STcxf1vtzkNNCnjmPejIYzLq02R0K+6AgmnT71rWzhFFo6OUz2Z3Qi6gk82pwB2ppWtvc/WrBDb+nJ1NK+j9OkWYj5Zg/A9CdfCh7hxrnOzxXoKixVWnNsGN9Yz4QbZz+SRtDWW0CbdCT0kvGne9NGrlv6Uhn6f9XF6JF73etK/MUWYdmo10eKHWch/39aih/qqttwDPQtAKizGsG7ReEoL7ZSSUZgqsDA=";


        options.IdentityProviderMetadataAddress = "https://localhost:5001/saml/metadata";

        options.ServiceProviderOptions = new SpOptions()
        {
            EntityId = "https://localhost:5003",
            SigningCertificate = new X509Certificate2("Resources/MyCert.pfx", "your_password"),
            MetadataPath = new PathString("/saml/metadata") 
        };

        options.SignInScheme = "Cookies";
        options.CallbackPath = new PathString("/signin-saml");

        options.SignedOutCallbackPath = "/signout-saml";

        
    });
builder.Services.AddTransient<ISamlNameIdGenerator, CustomNameIdGenerator>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

app.Run();
