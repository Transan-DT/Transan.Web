namespace Transcom.Web.Data.Forms
{
	public class FormTechnical
	{
		public TechnicalType ReportType { get; set; }

		public string ProblemDescription { get; set; }

		public string ProblemTarget { get; set; }

		public bool HasEvidence { get; set; }

		public string EvidenceDescription { get; set; }
	}

	public enum TechnicalType 
	{
		DiscordServer,
		DiscordBot,
		Website
	}
}
