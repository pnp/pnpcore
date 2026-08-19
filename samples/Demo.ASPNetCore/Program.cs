using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.ASPNetCore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string? environmentNameFromEnvFile = null;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                environmentNameFromEnvFile = GetEnvironmentNameFromEnvFile();

                if (!string.IsNullOrEmpty(environmentNameFromEnvFile))
                {
                    var currentEnvironment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (!string.Equals(currentEnvironment, environmentNameFromEnvFile, StringComparison.OrdinalIgnoreCase))
                    {
                        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentNameFromEnvFile);
                    }
                }
            }


            var builder = WebApplication.CreateBuilder(args);

            IEnumerable<string>? initialScopes = builder.Configuration.GetSection("DownstreamApis:MicrosoftGraph:Scopes").Get<IEnumerable<string>>();

            // Add Microsoft.Identity.Web services
            builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
                 .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                 .AddInMemoryTokenCaches();

            builder.Services.AddDownstreamApis(builder.Configuration.GetSection("DownstreamApis"));

            // Add the PnP Core SDK library
            builder.Services.AddPnPCore();
            builder.Services.Configure<PnPCoreOptions>(builder.Configuration.GetSection("PnPCore"));
            builder.Services.AddPnPCoreAuthentication();
            builder.Services.Configure<PnPCoreAuthenticationOptions>(builder.Configuration.GetSection("PnPCore"));
            

            //builder.Services.AddMvc();
            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();
            
            builder.Services.TryAddKeyedSingleton<string>("EnvironmentName", builder.Environment.EnvironmentName);
            

            var app = builder.Build();

            if (builder.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.UseStaticFiles();
#if DEBUG
            if (!app.Environment.IsDevelopment())
            {
                StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, builder.Configuration);
            }
#endif
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
        }

        public static string? GetEnvironmentNameFromEnvFile()
        {
            var testEnvironmentFile = Path.Combine(".", "env.txt");

            if (File.Exists(testEnvironmentFile))
            {
                var content = File.ReadAllText(testEnvironmentFile);
                if (!string.IsNullOrEmpty(content))
                {
                    return content.Trim();
                }
            }

            return null;
        }
    }
}
