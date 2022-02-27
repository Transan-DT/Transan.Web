using DSharpPlus;
using DSharpPlus.Entities;
using SocialGuard.Common.Data.Models;
using System.Linq;
using System.Text.RegularExpressions;
using Transan.Web.Data.Forms;

namespace Transan.Web;

public static class Utilities
{
	public static Regex SnowflakeRegex { get; } = new(@"(\d{17,21})", RegexOptions.Compiled);
		
	public const string SignatureFooter = "Transan (Web) - Powered by Nodsoft Systems";
	public const string AuditLogPrefix = "[Transan Web] - \n";

	public static string ToDisplayString(this Gender gender) => gender switch
	{
		Gender.Transgender => "Transgenre",
		Gender.NonBinary   => "Non-Binaire",
		Gender.GenderFluid => "Genderfluid",
		Gender.Cisgender   => "Homme/Femme Cisgenre",
		Gender.Questioning => "En Questionnement",
		Gender.Other       => "Autre",
		_                  => ""
	};

	public static string ToDisplayString(this ReportType reportType) => reportType switch
	{
		ReportType.Bullying    => "Harcèlement, Intimidation, Discrimination",
		ReportType.Spamming    => "Insultes, Spams",
		ReportType.Triggering  => "Sujet d'une conversation (sensible, TW/CW, illégal)",
		ReportType.Doxxing     => "Partage d'informations confidentielles",
		ReportType.HormonesDIY => "Partage d'information sur le THS DIY, ou les doses",
		ReportType.Other       => "Autre",
		_                      => ""
	};

	public static string ToDisplayString(this TechnicalType technicalType) => technicalType switch
	{
		TechnicalType.DiscordServer => "Serveur Discord",
		TechnicalType.DiscordBot    => "Bot(s) Discord",
		TechnicalType.Website       => "Site Web",
		TechnicalType.Other         => "Autre",
		_                           => ""
	};

	public static DiscordEmbedBuilder WithAuthor(this DiscordEmbedBuilder embed, DiscordUser user)
	{
		return embed.WithAuthor(user.GetFullUsername(), null, user.GetAvatarUrl(ImageFormat.Auto, 128));
	}

	public static string GetFullUsername(this DiscordUser user) => $"{user.Username}#{user.Discriminator}";


	#region SocialGuard

	public static string GetTrustlistLevelBootstrapColor(this TrustlistUser user) => user.Entries?.Max(e => e.EscalationLevel) switch
	{
		>= 3 => "danger",
		2    => "warning",
		1    => "info",
		_    => "success"
	};

	public static string GetTrustlistLevelDisplayString(this TrustlistUser user) => user.Entries?.Max(e => e.EscalationLevel) switch
	{
		>= 3 => "Dangereux",
		2    => "Méfiant",
		1    => "Suspicieux",
		_    => "Aucun"
	};

	#endregion // SocialGuard
}