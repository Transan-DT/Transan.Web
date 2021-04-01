using Discord;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using Transcom.Web.Data.Forms;

namespace Transcom.Web.Services
{
	public class SignupService
	{
		protected const int MaxEmbedContentLength = 1024;
		protected const string ContentTooLargeSubstituteText = "Contenu trop large ; Utilisez le site pour consulter ce champ.";

		private readonly IMongoCollection<FormSignup> signups;
		private readonly IDiscordClient discordClient;
		private readonly IConfiguration config;

		public delegate Task FormSubmittedEventHandler(object sender, FormSubmittedEventArgs args);
		public event FormSubmittedEventHandler FormSubmitted;

		public SignupService(IMongoClient mongoClient, IConfiguration config, IDiscordClient discordClient)
		{
			this.config = config;
			this.discordClient = discordClient;

			IMongoDatabase db = mongoClient.GetDatabase(config["MongoDb:Databases:Web"]);
			signups = db.GetCollection<FormSignup>("Signups");

			FormSubmitted += OnFormSubmitted;
		}

		public IQueryable<FormSignup> ListForms() => signups.AsQueryable();

		public async Task<FormSignup> GetUserFormAsync(ulong snowflake) => (await signups.FindAsync(f => f.UserSnowflake == snowflake)).FirstOrDefault();

		public async Task SubmitFormAsync(FormSignup form)
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
			IGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			return await guild.GetUserAsync(snowflake) is not null;
		}

		private async Task OnFormSubmitted(object _, FormSubmittedEventArgs args)
		{
			IGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			ITextChannel channel = await guild.GetTextChannelAsync(config.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"));
			IUser user = await guild.GetUserAsync(args.Form.UserSnowflake);

			EmbedBuilder builder = new EmbedBuilder()
				.WithTitle($"Formulaire d'inscription : {user.Username}")
				.WithAuthor(user)
				.WithFooter("Transcom (Web) - Powered by Nodsoft Systems")
				.WithUrl($"{config["Domain"]}/signup/view/{args.Form.Id}")
				.AddField("Orientation", args.Form.Orientation.ToDisplayString())
				.AddField("Présentation", args.Form.Presentation.Length < MaxEmbedContentLength ? args.Form.Presentation : ContentTooLargeSubstituteText)
				.AddField("Motivation", args.Form.Motivation.Length < MaxEmbedContentLength ? args.Form.Motivation : ContentTooLargeSubstituteText)
				.AddField($"Définition de {args.Form.Orientation.ToDisplayString()}", 
					args.Form.OrientationDefinition.Length < MaxEmbedContentLength ? args.Form.OrientationDefinition : ContentTooLargeSubstituteText);

			if (args.Form.Orientation is Orientation.Cisgender)
			{
				builder.AddField("Invité(e) par :", args.Form.ReferalUser);
			}

			await channel.SendMessageAsync($"{guild.GetRole(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")).Mention} Nouvelle demande d'inscription, de {user.Mention} :", embed: builder.Build());
		}
	}

	public class FormSubmittedEventArgs : EventArgs
	{
		public FormSignup Form { get; init; }
	}
}
