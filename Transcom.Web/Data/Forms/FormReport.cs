using System.ComponentModel.DataAnnotations;

namespace Transcom.Web.Data.Forms
{
	public record FormReport : FormBase
	{
		public ReportType ReportType { get; set; }

		[Required, MinLength(10), MaxLength(10000)]
		public string ProblemDescription { get; set; }

		[Required, MaxLength(100)]
		public string ProblemTarget { get; set; }

		public bool HasEvidence { get; set; }

		[Required, MinLength(10), MaxLength(10000)]
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
