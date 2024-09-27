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
        options.LicenseKey = "eyJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjQtMTAtMTlUMDA6MDA6MDAiLCJpYXQiOiIyMDI0LTA5LTE5VDA5OjE2OjA3Iiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.Px+W0ZpuAVL+jeIdppLYDcP6jrhBD7Llkv/56MMIcZ9D/VPZqZhhQx/wqvCfuGxI8QMg5/z0SXIn1IWPKeFcWdBYVyoixwzWbsi8or9g60rcePyyCn7ucPtu90A5mPs/mf6mQIc7qasFiL1RC7Dx0r153s3Im5WTOqr7O8gK45bNOqEninJ89pDxyX7p7I6a4BuSRxqDJYes9iP1NpL20YZbSrABW4cegAl7MsZmLc1zaEwYhN4+a3Yhap9JwKTUI3twQcOmKmPf1E7xeS5QunxIA4AvvicXuRjGMHnaUfzynEeTv9guiVh42bbbTxSLpVle7b6SeDcUrOBLAHzJHctLrio3zWYKdilnnmtDKlr8BMpmk9mH6pXsyC7fbG4wS0BbiAAday+PYjV/jOxdn8IK3xTrecp1+2w0D0obTLBTuVi1bWI5jeyHXXj9nBTAHnV01t9/X3jUSBDNHuRAltgBiAwuetRoRwXM4xSIdQS8UoXnFwoLNtgx5lUAaNdaNmnrDsA9jszZjDXgTZT4noQilbcC5wf+IhdgGyXMVSsOvhlS74Kcm1XBFhIzptQuF4QW+fy+qGLMZNr+1qUcJ/uTY2CRYMjcH2sHN9AJDWg89+UDTpm/Y+WhFAPsomYTXnpTjzDVzV1QULXDcL2lDQXfYsqytCrtvK+W5WikUMk=";


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
