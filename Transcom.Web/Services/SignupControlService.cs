using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;

namespace Transcom.Web.Services
{
	public class SignupControlService
	{
		internal DiscordGuild Guild { get; private set; }

		private readonly DiscordClient client;
		private static IConfiguration configuration;

		public SignupControlService(DiscordClient client, IConfiguration config)
		{
			this.client = client;
			configuration ??= config;

			Guild = client.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id")).GetAwaiter().GetResult();
		}

		public DiscordMember ResolveMember(ulong snowflake) => Guild.Members.TryGetValue(snowflake, out DiscordMember member) ? member : null;

		public IEnumerable<string> GetMemberRoles(DiscordMember member)
		{
			Dictionary<string, ulong> roles = new();
			configuration.GetSection("DiscordIntegration:Server:Roles").Bind(roles);
			return roles.Join(member.Roles, r1 => r1.Value, r2 => r2.Id, (r1, r2) => r1.Key);
		}

		public async Task AcceptNewMemberAsync(DiscordMember member, DiscordMember garantor)
		{
			await Guild.GetChannel(configuration.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"))
				.SendMessageAsync($"Inscription de {member.Mention} validée par {garantor.Mention}.", GenerateSignupReportEmbed(member, garantor));

			await member.RevokeRoleAsync(Guild.Roles.First(r => r.Key == configuration.GetValue<ulong>("DiscordIntegration:Server:Roles:Guest")).Value);
			await member.GrantRoleAsync(Guild.Roles.First(r => r.Key == configuration.GetValue<ulong>("DiscordIntegration:Server:Roles:Member")).Value);

			await Guild.GetChannel(configuration.GetValue<ulong>("DiscordIntegration:Server:Channels:Greet"))
				.SendMessageAsync($"Bienvenue à {member.Mention} !", GenerateWelcomeGreetEmbed(member));
			
			await member.SendMessageAsync($"Demande d'Inscription acceptée!", GenerateWelcomeDmEmbed());
		}



		private static DiscordEmbed GenerateSignupReportEmbed(DiscordMember member, DiscordMember garantor) => new DiscordEmbedBuilder()
			.WithTitle($"Demande d'inscription acceptée : {member.GetFullUsername()}")
			.WithColor(DiscordColor.Green)
			.WithFooter(Utilities.SignatureFooter)
			.WithAuthor(member)
			.WithUrl($"{configuration["Domain"]}/signup/view/{member.Id}")
			.AddField("Utilisateur", member.Mention)
			.AddField("Validation", garantor.Mention)
			.Build();

		private static DiscordEmbed GenerateWelcomeGreetEmbed(DiscordMember member) => new DiscordEmbedBuilder()
			.WithTitle($"Nouveau Membre : {member.Mention}")
			.WithDescription($"{member.Mention} est arrivé(e) sur le serveur. Souhaitez-lui la bienvenue!")
			.WithColor(DiscordColor.Green)
			.WithFooter(Utilities.SignatureFooter)
			.WithThumbnail(member.GetAvatarUrl(ImageFormat.Auto, 512))
			.Build();


		private static DiscordEmbed GenerateWelcomeDmEmbed() => new DiscordEmbedBuilder()
			.WithTitle($"Bienvenue !")
			.WithDescription("Votre Inscription vient d'être validée par la Modération. \n" +
				"Nous vous invitons à choisir vos rôles pour qu'on vous identifie correctement (obligatoire), puis vous présenter à la communauté. \n\n" +
				"Bienvenue sur Transgenres Community !")
			.WithColor(DiscordColor.Green)
			.WithFooter(Utilities.SignatureFooter)
			.AddField("🌸 Choisissez vos rôles", $"<#{configuration["DiscordIntegration:Server:Channels:RoleMenu"]}>")
			.AddField("🎨 Changez votre couleur", $"<#{configuration["DiscordIntegration:Server:Channels:ColorMenu"]}>")
			.AddField("🎤 Présentez vous", $"<#{configuration["DiscordIntegration:Server:Channels:Presentation"]}>")
			.Build();
	}
}
