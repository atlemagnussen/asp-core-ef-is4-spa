﻿using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Test.auth.Services;

namespace Test.auth.ServicesIdentity
{
    public class AzureSigningCredentialsStore : ISigningCredentialStore
    {
        private ILogger<AzureSigningCredentialsStore> _logger;
        private readonly IAzureKeyService _azureKeyService;

        public AzureSigningCredentialsStore(ILogger<AzureSigningCredentialsStore> logger,
            IAzureKeyService azureKeyService)
        {
            _logger = logger;
            _azureKeyService = azureKeyService;
        }
        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            _logger.LogInformation("AzureSigningCredentialsStore");
            var keys = await _azureKeyService.GetSigningKeysAsync();
            return new SigningCredentials(keys.Current.Key, keys.Current.AlgorithmString);
        }
    }
}
