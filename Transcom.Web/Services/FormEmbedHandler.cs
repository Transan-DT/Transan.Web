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

		public FormEmbedHandler(IConfiguration config, IDiscordClient discordClient, 
			FormService<FormSignup> signup,
			FormService<FormReport> report)
		{
			this.config = config;
			this.discordClient = discordClient;

			signup.FormSubmitted += OnSignupFormSubmittedAsync;
			report.FormSubmitted += OnReportFormSubmittedAsync;
		}


		private async Task OnSignupFormSubmittedAsync(object _, FormSubmittedEventArgs args)
		{
			if (args.Form is FormSignup form)
			{
				await SendSignupEmbedAsync(form);
			}
		}

		private async Task OnReportFormSubmittedAsync(object _, FormSubmittedEventArgs args)
		{
			if (args.Form is FormReport form)
			{
				await SendReportEmbedAsync(form);
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

		public async Task SendReportEmbedAsync(FormReport form)
		{
			IGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			ITextChannel channel = await guild.GetTextChannelAsync(config.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"));
			IUser user = await guild.GetUserAsync(form.UserSnowflake);

			EmbedBuilder builder = new EmbedBuilder()
				.WithTitle($"Formulaire d'inscription : {user.Username}")
				.WithAuthor(user)
				.WithFooter("Transcom (Web) - Powered by Nodsoft Systems")
				.WithUrl($"{config["Domain"]}/signup/view/{form.Id}")
				.AddField("Type de Signalement", form.ReportType.ToDisplayString())
				.AddField("Description du Problème", SubstituteOverflowText(form.ProblemDescription))
				.AddField("Cible du Signalement", SubstituteOverflowText(form.ProblemTarget))
				.AddField($"Preuves ?", form.HasEvidence ? "Oui" : "Non");

			if (!form.HasEvidence)
			{
				builder.AddField("Dates/Heures estimés", SubstituteOverflowText(form.EvidenceDescription));
			}

			await channel.SendMessageAsync($"{guild.GetRole(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")).Mention} Nouveau signalement, de {user.Mention} :", embed: builder.Build());
		}


		public static string SubstituteOverflowText(string text) => text.Length < MaxEmbedContentLength ? text : ContentTooLargeSubstituteText;
	}
}
