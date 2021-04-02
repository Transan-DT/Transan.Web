using Discord;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Transcom.Web.Data;
using Transcom.Web.Data.Forms;

namespace Transcom.Web
{
	public static class Utilities
	{
		public static Task Log(this ILogger logger, LogMessage logMessage) // Adapting MS's ILogger.Log() for Discord.NET events
		{
			logger.Log
			(
				logMessage.Severity switch
				{
					LogSeverity.Critical => LogLevel.Critical,
					LogSeverity.Debug => LogLevel.Debug,
					LogSeverity.Error => LogLevel.Error,
					LogSeverity.Info => LogLevel.Information,
					LogSeverity.Verbose => LogLevel.Trace,
					LogSeverity.Warning => LogLevel.Warning,
					_ => LogLevel.None
				},
				logMessage.Exception,
				logMessage.Message,
				logMessage.Source
			);

			return Task.CompletedTask;
		}

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
	}
}
