using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Transcom.Web.Data;
using Transcom.Web.Data.Forms;

namespace Transcom.Web
{
	public static class Utilities
	{
		public const string SignatureFooter = "Transcom (Web) - Powered by Nodsoft Systems";

		public static string ToDisplayString(this Orientation orientation) => orientation switch
		{
			Orientation.Transgender => "Transgenre",
			Orientation.NonBinary => "Non-Binaire",
			Orientation.GenderFluid => "Genderfluid",
			Orientation.Cisgender => "Homme/Femme Cisgenre",
			Orientation.Questioning => "En Questionnement",
			Orientation.Other => "Autre",
			_ => ""
		};

		public static string ToDisplayString(this ReportType reportType) => reportType switch
		{
			ReportType.Bullying => "Harcèlement, Intimidation, Discrimination",
			ReportType.Spamming => "Insultes, Spams",
			ReportType.Triggering => "Sujet d'une conversation (sensible, TW/CW, illégal)",
			ReportType.Doxxing => "Partage d'informations confidentielles",
			ReportType.HormonesDIY => "Partage d'information sur le THS DIY, ou les doses",
			ReportType.Other => "Autre",
			_ => ""
		};

		public static string ToDisplayString(this TechnicalType technicalType) => technicalType switch
		{
			TechnicalType.DiscordServer => "Serveur Discord",
			TechnicalType.DiscordBot => "Bot(s) Discord",
			TechnicalType.Website => "Site Web",
			TechnicalType.Other => "Autre",
			_ => ""
		};

		public static DiscordEmbedBuilder WithAuthor(this DiscordEmbedBuilder embed, DiscordUser user)
		{
			return embed.WithAuthor(user.GetFullUsername(), null, user.GetAvatarUrl(ImageFormat.Auto, 128));
		}

		public static string GetFullUsername(this DiscordUser user) => $"{user.Username}#{user.Discriminator}";
	}
}
