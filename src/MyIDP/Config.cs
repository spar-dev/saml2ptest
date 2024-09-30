// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace IdentityServer;
using Rsk.Saml;
using Rsk.Saml.Models;
using ServiceProvider = Rsk.Saml.Models.ServiceProvider;
using System.Security.Cryptography.X509Certificates;
using System.Security.Claims;


public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource()
            {
                Name = "verification",
                UserClaims = new List<string> 
                { 
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified
                }
            },
            new IdentityResource()
            {
                Name="user_info",
                DisplayName = "User Information",
                UserClaims= new List<string>{
                    "SURID", "UTYPE", "PID", "ULEVE"                 
                }
            },
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        { 
           new ApiScope(name: "appctr.readwrite",   displayName: "ReadWrite redirect info.")
           {
             UserClaims ={"SURID","UTYPE"}
           },
           new ApiScope(name: "appctr.manage",   displayName: "ReadWrite redirect info.")
           {
             UserClaims ={"SURID","UTYPE","name", "email"}
           },
           
           new ApiScope(name: "svapp.manage",    displayName: "Manages sv app")
            {
             UserClaims ={"SURID","UTYPE","ULEVE"}
           }, 
           new ApiScope(name: "svapp.readwrite",    displayName: "ReadWrite sv app")
            {
             UserClaims ={"SURID","UTYPE"}
           }, 
           new ApiScope(name: "svcommapp.manage",    displayName: "Manages sv comm app")
            {
             UserClaims ={"SURID","UTYPE","ULEVE"}
           }, 
           new ApiScope(name: "svcommapp.readwrite",    displayName: "ReadWrite sv comm app")
            {
             UserClaims ={"SURID","UTYPE"}
           },         
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("appctr", "API for Webapp redirection")
            {
                Scopes = { "appctr.readwrite","appctr.manage"}
            },
             new ApiResource("svappapi", "SV App Api")
            {
                Scopes = { "svapp.manage","svapp.readwrite" }
            },
            new ApiResource("svcommapi", "SV App Api")
            {
                Scopes = { "svcommapp.manage","svcommapp.readwrite" }
            },
        };

    public static IEnumerable<Client> Clients =>
        new Client[] 
        {
            new Client
            {
                ClientId = "client",

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                // scopes that client has access to
                AllowedScopes = { "api1", "appctr" }
            },
            new Client
            {
                ClientId = "testclient",

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,

                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                 
                    AlwaysSendClientClaims = true,
                    AllowOfflineAccess = true,
                // scopes that client has access to
                AllowedScopes = { 
                    IdentityServerConstants.StandardScopes.OpenId, 
                     IdentityServerConstants.StandardScopes.Profile,                      
                       "appctr.readwrite","appctr.manage","svapp.manage","svapp.readwrite","svcommapp.manage","svcommapp.readwrite" }
            },
            // interactive ASP.NET Core Web App
            new Client
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                
                // where to redirect to after login
                RedirectUris = { "https://localhost:5002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "verification"
                }
            },
            new Client()
            {
                ClientId = "https://localhost:5003",
                ProtocolType = IdentityServerConstants.ProtocolTypes.Saml2p,
                RedirectUris = new List<string>()
                {
                    "https://localhost:5003/signin-saml"
                },
                
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                     "user_info"
                }            
            },
        };
         public static IEnumerable<ServiceProvider> ServiceProvider => new List<ServiceProvider>()
    {
        
        new ServiceProvider()
        {
          EntityId = "https://localhost:5003",
            NameIdentifierFormat = SamlConstants.NameIdentifierFormats.Transient,
           
          AssertionConsumerServices = new List<Service>()
          {
              new Service(SamlConstants.BindingTypes.HttpPost, "https://localhost:5003/signin-saml")
          },
          SingleLogoutServices = new List<Service>()
          {
            new Service(SamlConstants.BindingTypes.HttpPost, "https://localhost:5003/signout-saml")  
          },
          SigningCertificates = new List<X509Certificate2>()
          {
              new X509Certificate2("Resources/MyCert.cer","your_password")
          }
        }
    };
}