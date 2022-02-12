using System;
using AspNet.Security.OAuth.Discord;
using DSharpPlus;
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
using Serilog.Extensions.Logging;
using SocialGuard.Common.Services;
using Transan.Web.Services;
using Transan.Web.Services.Authentication;
using Transan.Web.Services.SocialGuard;

namespace Transan.Web
{
	public class Startup
	{
		private readonly IWebHostEnvironment hostingEnvironment;

		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			this.hostingEnvironment = env;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddHttpContextAccessor();

			if (hostingEnvironment.IsProduction())
			{
				services.Configure<ForwardedHeadersOptions>(options =>
				{
					options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |	ForwardedHeaders.XForwardedProto;
					// Only loopback proxies are allowed by default.
					// Clear that restriction because forwarders are enabled by explicit
					// configuration.
					options.KnownNetworks.Clear();
					options.KnownProxies.Clear();
				});
			}

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
				options.Scope.Add("guilds");

				options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
				options.CorrelationCookie.SameSite = SameSiteMode.Lax;
			});

			services.AddAuthorization();

			services.AddHttpClient<RestClientBase>();

			services.AddSingleton<IMongoClient, MongoClient>(c => new(Configuration["MongoDb:ConnectionString"]));
			services.AddSingleton<HtmlContentFileLoader>();
			services.AddSingleton<GlossaryService>();
			services.AddSingleton<AuthService>();
			services.AddSingleton(typeof(FormService<>));
			services.AddSingleton<FormEmbedHandler>();
			
			services.AddScoped<SignupControlService>();

			services.AddHostedService<TrustlistCacheableClient>();
			services.AddSingleton<TrustlistCacheableClient>();

			services.AddSingleton(new DiscordClient(new()
			{
				TokenType = TokenType.Bot,
				Intents = DiscordIntents.All,
				Token = Configuration["DiscordIntegration:BotToken"],
				LoggerFactory = new SerilogLoggerFactory()
			}));

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
				app.UseForwardedHeaders();
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
