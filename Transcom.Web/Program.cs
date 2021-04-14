using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DSharpPlus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Transcom.Web.Services;

namespace Transcom.Web
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Debug()
	.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
#else
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
#endif
				.Enrich.FromLogContext()
				.Enrich.WithProperty("_Source", typeof(Program).Assembly.GetName())
				.WriteTo.Console()
				.CreateLogger();

			IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();

			// Prodding service to cause event hooks
			_ = scope.ServiceProvider.GetRequiredService<FormEmbedHandler>();

			await StartDiscordBotAsync(scope.ServiceProvider);
			await host.RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
					webBuilder.UseSerilog();
				});


		public static async Task StartDiscordBotAsync(IServiceProvider services)
		{
			DiscordClient client = services.GetRequiredService<DiscordClient>();

			await client.ConnectAsync();
		}
	}
}
