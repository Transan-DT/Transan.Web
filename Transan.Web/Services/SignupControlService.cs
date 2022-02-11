using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;


namespace Transan.Web.Services;


public class SignupControlService
{
	protected DiscordGuild Guild => _guild ??= _client.GetGuildAsync(_configuration.GetValue<ulong>("DiscordIntegration:Server:Id")).GetAwaiter().GetResult();
	private DiscordGuild _guild;

	private readonly DiscordClient _client;
	private static IConfiguration _configuration;

	public SignupControlService(DiscordClient client, IConfiguration config)
	{
		_client = client;
		_configuration ??= config;
	}

	public DiscordMember ResolveMember(ulong snowflake) => Guild.Members.GetValueOrDefault(snowflake);

	public IEnumerable<string> GetMemberRoles(DiscordMember member)
	{
		Dictionary<string, ulong> roles = new();
		_configuration.GetSection("DiscordIntegration:Server:Roles").Bind(roles);
		return roles.Join(member.Roles, r1 => r1.Value, r2 => r2.Id, (r1, r2) => r1.Key);
	}

	public async Task AcceptNewMemberAsync(DiscordMember member, DiscordMember garantor)
	{
		await Guild.GetChannel(_configuration.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"))
			.SendMessageAsync($"Inscription de {member.Mention} validée par {garantor.Mention}.", GenerateSignupReportEmbed(member, garantor));

		await member.RevokeRoleAsync(Guild.Roles.First(r => r.Key == _configuration.GetValue<ulong>("DiscordIntegration:Server:Roles:Guest")).Value);
		await member.GrantRoleAsync(Guild.Roles.First(r => r.Key == _configuration.GetValue<ulong>("DiscordIntegration:Server:Roles:Member")).Value);

		_ = await Guild.GetChannel(_configuration.GetValue<ulong>("DiscordIntegration:Server:Channels:Greet"))
			.SendMessageAsync($"Bienvenue à {member.Mention} !", GenerateWelcomeGreetEmbed(member));

		_ = await member.SendMessageAsync($"Demande d'Inscription acceptée!", GenerateWelcomeDmEmbed());
	}

	public async Task RejectNewMemberAsync(DiscordMember member, DiscordMember garantor, string reason, RejectAction action)
	{
		await Guild.GetChannel(_configuration.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"))
			.SendMessageAsync($"Inscription de {member.Mention} refusée par {garantor.Mention}.", GenerateDenyReportEmbed(member, garantor, reason, action));

		await member.SendMessageAsync("Demande d'Inscription refusée.", GenerateDenyDmEmbed(reason, action));

		if (action is RejectAction.Ban)
		{
			await member.BanAsync(0, Utilities.AuditLogPrefix + reason);
		}
		else if (action is RejectAction.Kick)
		{
			await member.RemoveAsync(Utilities.AuditLogPrefix + reason);
		}
	}

	public async Task ControlNewMemberAsync(DiscordMember member, DiscordMember garantor, string reason)
	{
		DiscordChannel channel = await Guild.CreateChannelAsync(
			name: $"🔒🔎-control-{member.Id:X}",
			type: ChannelType.Text,
			parent: Guild.GetChannel(_configuration.GetValue<ulong>("DiscordIntegration:Server:Categories:Control")),
			overwrites: new[]
			{
				new DiscordOverwriteBuilder(Guild.EveryoneRole).Deny(Permissions.AccessChannels),
				new DiscordOverwriteBuilder(Guild.Roles.First(r => r.Key == _configuration.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")).Value).Allow(Permissions.AccessChannels | Permissions.ReadMessageHistory | Permissions.SendMessages),
				new DiscordOverwriteBuilder(member).Allow(Permissions.AccessChannels | Permissions.ReadMessageHistory | Permissions.SendMessages)
			});

		await Guild.GetChannel(_configuration.GetValue<ulong>("DiscordIntegration:Server:Channels:Signup"))
			.SendMessageAsync($"Mise sous contrôle de {member.Mention} par {garantor.Mention}.", GenerateControlReportEmbed(member, garantor, channel, reason));

		await member.SendMessageAsync(GenerateControlDmEmbed(channel, reason));
	}




	#region Embeds

	private static DiscordEmbed GenerateSignupReportEmbed(DiscordMember member, DiscordUser garantor) => new DiscordEmbedBuilder()
		.WithTitle($"Demande d'inscription acceptée : {member.Nickname}")
		.WithColor(DiscordColor.Green)
		.WithFooter(Utilities.SignatureFooter)
		.WithAuthor(member)
		.WithUrl($"{_configuration["Domain"]}/signup/view/{member.Id}")
		.AddField("Utilisateur", member.Mention)
		.AddField("Validation", garantor.Mention)
		.Build();

	private static DiscordEmbed GenerateWelcomeGreetEmbed(DiscordMember member) => new DiscordEmbedBuilder()
		.WithTitle($"Nouveau Membre : {member.Nickname}")
		.WithDescription($"{member.Mention} est arrivé(e) sur le serveur. Souhaitez-lui la bienvenue!")
		.WithColor(DiscordColor.Green)
		.WithFooter(Utilities.SignatureFooter)
		.WithThumbnail(member.GetAvatarUrl(ImageFormat.Auto, 512))
		.Build();

	private static DiscordEmbed GenerateWelcomeDmEmbed() => new DiscordEmbedBuilder()
		.WithTitle("Bienvenue !")
		.WithDescription("Votre Inscription vient d'être validée par l'équipe de Modération. \n" +
			"Nous vous invitons dès à présent à choisir vos rôles pour que nous puissions vous identifier correctement, puis vous présenter à la communauté. \n\n" +
			"Bienvenue sur Le Transanctuaire !")
		.WithColor(DiscordColor.Green)
		.WithFooter(Utilities.SignatureFooter)
		.AddField("🌸 Choisissez vos rôles", $"<#{_configuration["DiscordIntegration:Server:Channels:RoleMenu"]}>")
		.AddField("🎤 Présentez vous", $"<#{_configuration["DiscordIntegration:Server:Channels:Presentation"]}>")
		.Build();

	private static DiscordEmbed GenerateDenyReportEmbed(DiscordMember member, DiscordUser garantor, string reason, RejectAction action)
	{
		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle($"Demande d'inscription refusée : {member.Nickname}")
			.WithColor(DiscordColor.Red)
			.WithFooter(Utilities.SignatureFooter)
			.WithAuthor(member)
			.WithUrl($"{_configuration["Domain"]}/signup/view/{member.Id}")
			.AddField("Utilisateur", member.Mention)
			.AddField("Validation", garantor.Mention);

		if (!string.IsNullOrWhiteSpace(reason))
		{
			embed.AddField("Motif", reason);
		}

		if (action is RejectAction.Ban)
		{
			embed.AddField("Bannissement", "L'utilisateur a été banni du serveur.");
		}

		if (action is RejectAction.Kick)
		{
			embed.AddField("Exclusion", "L'utilisateur a été exclu du serveur.");
		}

		return embed.Build();
	}

	private static DiscordEmbed GenerateDenyDmEmbed(string reason, RejectAction action)
	{
		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle("Inscription refusée")
			.WithDescription("Désolé, votre demande d'inscription a été refusée par l'équipe de Modération.")
			.WithColor(DiscordColor.Red)
			.WithFooter(Utilities.SignatureFooter);

		if (!string.IsNullOrWhiteSpace(reason))
		{
			embed.AddField("Motif", reason);
		}

		if (action is RejectAction.Kick)
		{
			embed.AddField("Exclusion", "Suite à votre refus, vous avez été exclu(e) du serveur. Désolé.");
		}
		else if (action is RejectAction.Ban)
		{
			embed.AddField("Bannissement", "Suite à votre refus, vous avez été banni(e) du serveur. Désolé.");
		}
		else
		{
			embed.AddField("Une erreur ?", "Si vous considérez ce refus comme étant une erreur, nous vous suggérons de prendre contact avec notre équipe de Modération.");
		}

		return embed.Build();
	}

	private static DiscordEmbed GenerateControlReportEmbed(DiscordUser member, DiscordUser garantor, DiscordChannel channel, string reason)
	{
		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle("Mise sous contrôle")
			.WithColor(DiscordColor.Orange)
			.WithFooter(Utilities.SignatureFooter)
			.AddField("Utilisateur", member.Mention)
			.AddField("Validation", garantor.Mention)
			.AddField("Channel", channel.Mention);

		if (!string.IsNullOrWhiteSpace(reason))
		{
			embed.AddField("Raison", reason);
		}

		return embed.Build();
	}

	private static DiscordEmbed GenerateControlDmEmbed(DiscordChannel channel, string remarks)
	{
		DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
			.WithTitle("Petit soucis...")
			.WithDescription("Nous avons un problème avec votre demande d'inscription. \nVeuillez prendre contact avec la modération dans le channel ci-dessous.")
			.WithColor(DiscordColor.Orange)
			.WithFooter(Utilities.SignatureFooter)
			.AddField("Channel", channel.Mention);

		if (!string.IsNullOrWhiteSpace(remarks))
		{
			embed.AddField("Remarques", remarks);
		}

		return embed.Build();
	}

	#endregion //Embeds


	public enum RejectAction
	{
		None,
		Kick,
		Ban
	}
}
