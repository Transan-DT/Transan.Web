using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;



namespace Transcom.Web
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();

			await Task.WhenAll(StartDiscordBotAsync(scope.ServiceProvider), host.RunAsync());
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});


		public static async Task StartDiscordBotAsync(IServiceProvider services)
		{
			IConfiguration config = services.GetRequiredService<IConfiguration>();
			DiscordSocketClient discordClient = services.GetRequiredService<IDiscordClient>() as DiscordSocketClient;
			ILogger discordLogger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(discordClient));

			discordClient.Log += discordLogger.Log;
			await discordClient.LoginAsync(TokenType.Bot, config["DiscordIntegration:BotToken"]);
			await discordClient.StartAsync();
		}
	}
}
