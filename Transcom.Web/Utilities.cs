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
			Orientation.TransgenderMale => "Homme Transgenre",
			Orientation.TransgenderFemale => "Femme Transgenre",
			Orientation.NonBinary => "Non-Binaire",
			Orientation.GenderFluid => "Genderfluid",
			Orientation.Cisgender => "Homme/Femme Cisgenre",
			Orientation.Questioning => "En Questionnement",
			Orientation.Other => "Autre",
			_ => ""
		};
	}
}
