using Discord;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Transcom.Web.Data.Forms;



namespace Transcom.Web.Services
{
	public class FormEmbedHandler
	{
		protected const int MaxEmbedContentLength = 1024;
		protected const string ContentTooLargeSubstituteText = "Contenu trop large ; Utilisez le site pour consulter ce champ.";

		private readonly IConfiguration config;
		private readonly IDiscordClient discordClient;

		public FormEmbedHandler(IConfiguration config, IDiscordClient discordClient, FormService<FormSignup> signup)
		{
			this.config = config;
			this.discordClient = discordClient;
			signup.FormSubmitted += OnSignupFormSubmittedAsync;
		}

		private async Task OnSignupFormSubmittedAsync(object _, FormSubmittedEventArgs args)
		{
			if (args.Form is FormSignup form)
			{
				await SendSignupEmbedAsync(form);
			}
		}

		public async Task SendSignupEmbedAsync(FormSignup form)
		{
			IGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			ITextChannel channel = await guild.GetTextChannelAsync(config.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"));
			IUser user = await guild.GetUserAsync(form.UserSnowflake);

			EmbedBuilder builder = new EmbedBuilder()
			.WithTitle($"Formulaire d'inscription : {user.Username}")
			.WithAuthor(user)
			.WithFooter("Transcom (Web) - Powered by Nodsoft Systems")
			.WithUrl($"{config["Domain"]}/signup/view/{form.UserSnowflake}")
			.AddField("Orientation", form.Orientation.ToDisplayString())
			.AddField("Présentation", form.Presentation.Length < MaxEmbedContentLength ? form.Presentation : ContentTooLargeSubstituteText)
			.AddField("Motivation", form.Motivation.Length < MaxEmbedContentLength ? form.Motivation : ContentTooLargeSubstituteText)
			.AddField($"Définition de {form.Orientation.ToDisplayString()}",
				form.OrientationDefinition.Length < MaxEmbedContentLength ? form.OrientationDefinition : ContentTooLargeSubstituteText);

			if (form.Orientation is Orientation.Cisgender)
			{
				builder.AddField("Invité(e) par :", form.ReferalUser);
			}

			await channel.SendMessageAsync($"{guild.GetRole(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")).Mention} Nouvelle demande d'inscription, de {user.Mention} :", embed: builder.Build());
		}
	}
}
