using Discord;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using Transcom.Web.Data;

namespace Transcom.Web.Services
{
	public class SignupService
	{
		private readonly IMongoCollection<DiscordSignupForm> signups;
		private readonly IDiscordClient discordClient;
		private readonly IConfiguration config;

		public delegate Task FormSubmittedEventHandler(object sender, FormSubmittedEventArgs args);
		public event FormSubmittedEventHandler FormSubmitted;

		public SignupService(IMongoClient mongoClient, IConfiguration config, IDiscordClient discordClient)
		{
			this.config = config;
			this.discordClient = discordClient;

			IMongoDatabase db = mongoClient.GetDatabase(config["MongoDb:Databases:Web"]);
			signups = db.GetCollection<DiscordSignupForm>("Signups");

			FormSubmitted += OnFormSubmitted;
		}

		public IQueryable<DiscordSignupForm> ListForms() => signups.AsQueryable();

		public async Task<DiscordSignupForm> GetUserFormAsync(ulong snowflake) => (await signups.FindAsync(f => f.UserSnowflake == snowflake)).First();

		public async Task SubmitFormAsync(DiscordSignupForm form)
		{
			if (!await UserIsOnServerAsync(form.UserSnowflake))
			{
				throw new ApplicationException("User is not on Server.");
			}

			await signups.InsertOneAsync(form with
			{
				SubmittedAt = DateTime.UtcNow
			});

			FormSubmitted?.Invoke(this, new() { Form = form });
		}


		public async Task<bool> UserIsOnServerAsync(ulong snowflake)
		{
			IGuild guild = await discordClient.GetGuildAsync(Convert.ToUInt64(config["DiscordIntegration:Server:Id"]));
			return guild.GetUserAsync(snowflake) is not null;
		}

		private async Task OnFormSubmitted(object _, FormSubmittedEventArgs args)
		{
			IGuild guild = await discordClient.GetGuildAsync(Convert.ToUInt64(config["DiscordIntegration:Server:Id"]));
			ITextChannel channel = await guild.GetTextChannelAsync(Convert.ToUInt64(config["DiscordIntegration:Channels:Signup"]));
			IUser user = await guild.GetUserAsync(args.Form.UserSnowflake);

			EmbedBuilder builder = new EmbedBuilder()
				.WithTitle($"Formulaire d'inscription : {user.Username}")
				.WithAuthor(user)
				.WithFooter("Transcom (Web) - Powered by Nodsoft Systems")
				.WithTimestamp(new(args.Form.SubmittedAt))
				.WithUrl($"{config["Domain"]}/signup/view/{args.Form.UserSnowflake}")
				.AddField("Orientation", args.Form.Orientation.ToDisplayString())
				.AddField("Présentation", args.Form.Presentation)
				.AddField("Motivation", args.Form.Motivation)
				.AddField($"Définition de {args.Form.Orientation}", args.Form.OrientationDefinition);

			if (args.Form.Orientation is Orientation.Cisgender)
			{
				builder.AddField("Invité(e) par :", args.Form.ReferalUser);
			}

			await channel.SendMessageAsync($"{guild.GetRole(Convert.ToUInt64(config["DiscordIntegration:Roles:Mod"]))} Nouvelle demande d'inscription :", embed: builder.Build());
		}
	}

	public class FormSubmittedEventArgs : EventArgs
	{
		public DiscordSignupForm Form { get; init; }
	}
}
