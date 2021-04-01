namespace Transcom.Web.Data.Forms
{
	public class FormReport
	{
		public ReportType ReportType { get; set; }

		public string ProblemDescription { get; set; }

		public string ProblemTarget { get; set; }

		public bool HasEvidence { get; set; }

		public string EvidenceDescription { get; set; }
	}

	public enum ReportType 
	{
		Unknown,
		Bullying,
		Spamming,
		Triggering,
		Doxxing,
		HormonesDIY,
		Other
	}
}
