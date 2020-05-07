﻿using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;

namespace Test.auth
{
    public class Config
    {
        public static string WebClientName = "webclient";
        public static string MobileClientId = "mobileapp";
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("bankApi", "Customer Api for Bank", new[] { JwtClaimTypes.Email })
                {
                    Scopes = { new Scope("api.read"), new Scope("api.write") }
                }
            };
        }

        public static IEnumerable<Client> GetClients(string allowedClientUrl)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "bankApi" }
                },
                GetWebClient(allowedClientUrl),
                GetMobileClient()
            };
        }

        private static Client GetWebClient(string allowedClientUrl)
        {
            var allowedUrls = allowedClientUrl.Split(';').ToList();
            allowedUrls.Add("http://localhost:8080");
            var redirects = new List<string>();
            foreach (var url in allowedUrls)
            {
                redirects.Add(url);
                redirects.Add($"{url}/callback.html");
                redirects.Add($"{url}/silent-renew.html");
            }
            return new Client
            {
                ClientId = WebClientName,
                ClientName = "SPA web client",

                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                RequireConsent = false,
                //RefreshTokenUsage = TokenUsage.OneTimeOnly,
                //UpdateAccessTokenClaimsOnRefresh = true,
                // RefreshTokenExpiration = TokenExpiration.Sliding,
                
                AccessTokenLifetime = 20*60, //24 * 3600,

                RedirectUris = redirects,
                PostLogoutRedirectUris = allowedUrls,
                AllowedCorsOrigins = allowedUrls,

                AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "roles",
                        "api.read",
                        "api.write"
                    },
                AllowOfflineAccess = false
            };
        }
        public static Client GetMobileClient()
        {
            return new Client
            {
                ClientId = MobileClientId,
                ClientName = "DigiLEAN mobile app",

                RedirectUris = { "com.companyname.mobileapp://callback" },
                PostLogoutRedirectUris = { "com.companyname.mobileapp://callback" },
                BackChannelLogoutUri = "com.companyname.mobileapp://callback",
                BackChannelLogoutSessionRequired = true,

                RequireClientSecret = false,
                RequireConsent = false,

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                AllowedScopes = { "openid", "profile", "email", "roles", "api.read" },
                
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                UpdateAccessTokenClaimsOnRefresh = true,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 14 * 24 * 60 * 60
            };
        }
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "roles",
                    DisplayName = "Roles",
                    Description = "Allow the service access to your user roles.",
                    UserClaims = new[] { JwtClaimTypes.Role },
                    ShowInDiscoveryDocument = true,
                    Required = true,
                    Emphasize = true
                }
            };
        }
    }
}
