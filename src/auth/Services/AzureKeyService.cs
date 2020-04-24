﻿using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using IdentityServer4;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using Test.auth.Models;

namespace Test.auth.Services
{
    public interface IAzureKeyService
    {
        Task<RsaSigningKeyModel> GetRsaSigningKey();
        EcSigningKeyModel GetEcSigningKey();
    }
    public class AzureKeyService : IAzureKeyService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureKeyService> _logger;
        private readonly string _vaultUrl;
        private readonly KeyClient _keyClient;
        private readonly KeyVaultClient _keyVaultClient;

        private static string RsaKeyName = "rsa-2048-core-auth";
        private static string EcKeyName = "ec-2048-core-auth";

        public AzureKeyService(IConfiguration configuration,
            ILogger<AzureKeyService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _vaultUrl = $"https://{configuration["KeyVaultName"]}.vault.azure.net/";
            var vaultUri = new Uri(_vaultUrl);
            var tokenCredential = new DefaultAzureCredential();
            //var tokenCredential = new ManagedIdentityCredential();
            _keyClient = new KeyClient(vaultUri, tokenCredential, new KeyClientOptions);

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            _keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));
        }

        public async Task<RsaSigningKeyModel> GetRsaSigningKey()
        {
            _logger.LogInformation("Start GetRsaSigningKey");
            var model = new RsaSigningKeyModel();
            try
            {
                var keyBundle = await _keyVaultClient.GetKeyAsync(_vaultUrl, RsaKeyName);
                model.Raw = keyBundle.Key.ToRSA().ToXmlString(true);

                if (keyBundle != null)
                {
                    var rsa = keyBundle.Key.ToRSA();
                    model.Key = new RsaSecurityKey(rsa);
                    model.Algorithm = IdentityServerConstants.RsaSigningAlgorithm.PS256;
                }
                else
                {
                    _logger.LogError("GetRsaSigningKey keyFrom was null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRsaSigningKey Error GetRsaSigningKey", ex.Message);
            }
            return model;
        }

        public EcSigningKeyModel GetEcSigningKey()
        {
            _logger.LogInformation("Start GetEcSigningKey");
            var model = new EcSigningKeyModel();
            try
            {
                string raw = _configuration.GetValue<string>(EcKeyName);
                model.Raw = raw;

                _logger.LogInformation($"GetEcSigningKey raw config key; {raw}");
                var keyFrom = _keyClient.GetKey(EcKeyName);
                if (keyFrom != null)
                {
                    _logger.LogInformation($"{keyFrom.Value.KeyType}");
                    var ec = keyFrom.Value.Key.ToECDsa();
                    _logger.LogInformation($"Ec: {ec.ToString()}");
                    model.Key = new ECDsaSecurityKey(ec);
                    model.Algorithm = IdentityServerConstants.ECDsaSigningAlgorithm.ES256;
                }
                else
                {
                    _logger.LogError("GetEcSigningKey keyFrom was null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetEcSigningKey", ex);
            }
            return model;
        }
    }
}