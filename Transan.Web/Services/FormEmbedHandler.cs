using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Transan.Web.Data.Forms;



namespace Transan.Web.Services
{
	public class FormEmbedHandler
	{
		protected const int MaxEmbedContentLength = 1024;
		protected const string ContentTooLargeSubstituteText = "Contenu trop large ; Utilisez le site pour consulter ce champ.";
		private readonly IConfiguration config;
		private readonly DiscordClient discordClient;

		public FormEmbedHandler(IConfiguration config, DiscordClient discordClient, 
			FormService<FormSignup> signup,
			FormService<FormReport> report,
			FormService<FormTechnical> technical)
		{
			this.config = config;
			this.discordClient = discordClient;

			signup.FormSubmitted += OnSignupFormSubmittedAsync;
			report.FormSubmitted += OnReportFormSubmittedAsync;
			technical.FormSubmitted += OnTechnicalFormSubmittedAsync;
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

		private async Task OnTechnicalFormSubmittedAsync(object _, FormSubmittedEventArgs args)
		{
			if (args.Form is FormTechnical form)
			{
				await SendTechnicalEmbedAsync(form);
			}
		}

		public async Task SendSignupEmbedAsync(FormSignup form)
		{
			DiscordGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			DiscordChannel channel = guild.GetChannel(config.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"));
			DiscordUser user = await discordClient.GetUserAsync(form.UserSnowflake);

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
			.WithTitle($"Formulaire d'inscription : {user.GetFullUsername()}")
			.WithAuthor(user)
			.WithFooter(Utilities.SignatureFooter)
			.WithUrl($"{config["Domain"]}/signup/view/{form.UserSnowflake}")
			.AddField("Orientation", form.Gender.ToDisplayString())
			.AddField("Présentation", form.Presentation.Length < MaxEmbedContentLength ? form.Presentation : ContentTooLargeSubstituteText)
			.AddField("Motivation", form.Motivation.Length < MaxEmbedContentLength ? form.Motivation : ContentTooLargeSubstituteText)
			.AddField($"Définition de {form.Gender.ToDisplayString()}",
				form.OrientationDefinition.Length < MaxEmbedContentLength ? form.OrientationDefinition : ContentTooLargeSubstituteText);

			if (form.Gender is Gender.Cisgender)
			{
				builder.AddField("Invité(e) par :", form.ReferalUser);
			}

			await channel.SendMessageAsync($"{guild.GetRole(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")).Mention} Nouvelle demande d'inscription, de {user.Mention} :", embed: builder.Build());
		}

		public async Task SendReportEmbedAsync(FormReport form)
		{
			DiscordGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			DiscordChannel channel = guild.GetChannel(config.GetValue<ulong>("DiscordIntegration:Server:Channels:Report"));
			DiscordUser user = await discordClient.GetUserAsync(form.UserSnowflake);

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
				.WithTitle("Formulaire de Signalement")
				.WithAuthor(user)
				.WithFooter(Utilities.SignatureFooter)
				//.WithUrl($"{config["Domain"]}/signup/view/{form.Id}")
				.AddField("Type de Signalement", form.ReportType.ToDisplayString())
				.AddField("Cible du Signalement", SubstituteOverflowText(form.ProblemTarget))
				.AddField("Description du Problème", SubstituteOverflowText(form.ProblemDescription))
				.AddField($"Preuves ?", form.HasEvidence ? "Oui" : "Non");

			if (!form.HasEvidence)
			{
				builder.AddField("Dates/Heures estimés", SubstituteOverflowText(form.EvidenceDescription));
			}

			await channel.SendMessageAsync($"{guild.GetRole(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")).Mention} Nouveau signalement, de {user.Mention} :", embed: builder.Build());
		}

		public async Task SendTechnicalEmbedAsync(FormTechnical form)
		{
			DiscordGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			DiscordChannel channel = guild.GetChannel(config.GetValue<ulong>("DiscordIntegration:Server:Channels:Report"));
			DiscordUser user = await discordClient.GetUserAsync(form.UserSnowflake);

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
				.WithTitle("Incident / Problème Technique")
				.WithAuthor(user)
				.WithFooter(Utilities.SignatureFooter)
				//.WithUrl($"{config["Domain"]}/signup/view/{form.Id}")
				.AddField("Type d'Incident", form.IssueType.ToDisplayString())
				.AddField("Nature/Cible du problème", SubstituteOverflowText(form.IssueTarget))
				.AddField("Description du Problème", SubstituteOverflowText(form.ProblemDescription));

			await channel.SendMessageAsync($"{guild.GetRole(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Admin")).Mention} Nouveau signalement, de {user.Mention} :", embed: builder.Build());
		}


		public static string SubstituteOverflowText(string text) => text.Length < MaxEmbedContentLength ? text : ContentTooLargeSubstituteText;
	}
}
