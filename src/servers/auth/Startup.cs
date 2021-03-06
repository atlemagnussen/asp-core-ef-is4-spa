using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Test.auth.Extentions;
using Test.auth.Services;
using Test.core.Services;
using Test.dataaccess;
using Test.model.Users;

namespace Test.auth
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; set; }

        public Startup(IConfiguration configuration,
            IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // cookie policy to deal with temporary browser incompatibilities
            services.AddSameSiteCookiePolicy();

            var configAzAd = Configuration.GetSection("AzureAd");
            var configAzKv = Configuration.GetSection("AzureKeyVault");
            services.AddServices(configAzAd, configAzKv);

            services.AddIdentityServerConfig(Configuration, Environment, configAzAd);
            services.AddCommonIdentitySettings();
            services.AddCommonDataProtection(configAzKv, Environment.IsDevelopment());

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequiresAdmin", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.Role, SystemRoles.Admin);
                }
                );
            });
            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Clients", "RequiresAdmin");
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            // app.UseDatabaseErrorPage();

            app.UseHsts();
            app.UseHttpsRedirection();
            
            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseRouting();
            
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
