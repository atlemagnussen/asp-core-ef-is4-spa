﻿using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Test.auth.Models;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Test.auth.Services
{
    public interface ILogoutService
    {
        public Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId, ClaimsPrincipal user, HttpContext httpContext);
        public Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId, ClaimsPrincipal user);
    }

    public class LogoutService : ILogoutService
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;

        public LogoutService(IIdentityServerInteractionService interaction,
            IClientStore clientStore)
        {
            _interaction = interaction;
            _clientStore = clientStore;
        }
        public async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId, ClaimsPrincipal user, HttpContext httpContext)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var postLogoutUri = await GetPostLogoutUrl(logout);
            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = postLogoutUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (user?.Identity.IsAuthenticated == true)
            {
                var idp = user.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await httpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
        public async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId, ClaimsPrincipal user)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (user?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<string> GetPostLogoutUrl(LogoutRequest logout)
        {
            if (logout == null)
                return null;

            if (!string.IsNullOrEmpty(logout.PostLogoutRedirectUri))
                return logout.PostLogoutRedirectUri;

            var clientId = GetClientId(logout);
            if (clientId == null)
                return null;

            var client = await _clientStore.FindClientByIdAsync(clientId);
            if (client == null)
                return null;

            return client.PostLogoutRedirectUris.FirstOrDefault();
        }

        private string GetClientId(LogoutRequest logout)
        {
            if (!string.IsNullOrEmpty(logout.ClientId))
                return logout.ClientId;

            if (logout.ClientIds != null)
            {
                var clientIds = logout.ClientIds.ToArray();
                if (clientIds.Length == 1)
                    return clientIds.First();
            }
            return null;
        }
    }
}
