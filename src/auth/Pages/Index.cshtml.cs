using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Test.auth.Models;
using Test.auth.Services;

namespace Test.auth.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAzureKeyService _azureKeyService;
        private readonly IClientStore _clientStore;

        public IndexModel(IWebHostEnvironment environment,
            ILogger<IndexModel> logger,
            IConfiguration configuration,
            IAzureKeyService azureKeyService,
            IClientStore clientStore)
        {
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
            _azureKeyService = azureKeyService;
            _clientStore = clientStore;
        }
        public string ConnectionString1 { get; set; }
        public string ConnectionString2 { get; set; }
        public string AllowedClientUrl { get; set; }
        public string AzureAdClientId { get; set; }
        public string ClientUrl { get; set; }

        public string Secret1 { get; set; }
        public string Secret2 { get; set; }
        public string Secret3 { get; set; }
        public string Secret4 { get; set; }

        public bool IsDevelopment { get; set; }

        public string LoggedInUser { get; set; }
        public string Role { get; set; }
        public Client WebClient { get; set; }
        public EcSigningKeys SigningKeys { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Index start");
            var allClaims = User.Claims.ToList();
            LoggedInUser = allClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.PreferredUserName)?.Value;
            Role = allClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.Role)?.Value;
            // var identities = User.Identities.ToList();
            try
            {
                WebClient = await _clientStore.FindClientByIdAsync(Config.WebClientName);
                ConnectionString1 = GetConStrStripPw("AuthDb"); //_configuration.GetConnectionString("Test");
                ConnectionString2 = GetConStrStripPw("BankDatabase"); //_configuration["ConnectionStrings:Test"];
                AllowedClientUrl = _configuration.GetValue<string>("AllowedClientUrl");
                AzureAdClientId = _configuration.GetValue<string>("AzureAd:ClientId");
                ClientUrl = _configuration.GetValue<string>("AllowedClientUrl");
                Secret1 = _configuration["SecretName"];
                Secret2 = _configuration["Section:SecretName"];
                Secret3 = _configuration.GetSection("Section")["SecretName"];
                Secret4 = _configuration.GetValue<string>("SecretName");
                IsDevelopment = _environment.IsDevelopment();
                SigningKeys = await _azureKeyService.GetSigningKeysAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Terrible ERROR IN INDEX", ex);
            }
            
            return Page();
        }

        private string GetConStrStripPw(string name) {
            var connectionString = _configuration.GetConnectionString(name);
            string[] settings = connectionString.Split(';');
            string conString = string.Empty;
            if (settings.Length > 0) {
                foreach (var setting in settings) {
                    if (setting.ToLower().StartsWith("password"))
                        continue;
                    conString = $"{conString}{setting};";
                }
            }
            return conString;
        }
    }
}
