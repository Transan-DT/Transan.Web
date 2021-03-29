using AspNet.Security.OAuth.Discord;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Transcom.Web.Services;
using Transcom.Web.Services.Authentication;

namespace Transcom.Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor();

			services.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
			})
			.AddCookie(options =>
			{

			})
			.AddDiscord(options =>
			{
				options.ClientId = Configuration["DiscordAuth:ClientId"];
				options.ClientSecret = Configuration["DiscordAuth:ClientSecret"];
				options.CallbackPath = "/signin-oauth2";

				options.Scope.Add("identify");
				options.Scope.Add("email");
				options.Scope.Add("connections");
				options.Scope.Add("guilds");
				options.Scope.Add("guilds.join");
			});

			services.AddAuthorization();

			services.AddSingleton<TeamPageLoaderService>();
			services.AddSingleton<GlossaryService>();
			services.AddSingleton<AuthDbService>();
			services.AddSingleton<IMongoClient, MongoClient>(c => new(Configuration["MongoDb:ConnectionString"]));
			services.AddSingleton<DiscordSocketClient>();
			services.AddSingleton<IDiscordClient, DiscordSocketClient>();

			services.AddScoped<IClaimsTransformation, WebAppClaims>();
		}


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			if (env.IsProduction()) // Nginx configuration step
			{
				app.UseForwardedHeaders(new ForwardedHeadersOptions
				{
					ForwardedHeaders = ForwardedHeaders.All
				});
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
