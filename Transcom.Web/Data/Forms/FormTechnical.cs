using System.ComponentModel.DataAnnotations;



namespace Transcom.Web.Data.Forms
{
	public record FormTechnical : FormBase
	{
		public TechnicalType IssueType { get; set; }

		[Required, MaxLength(200)]
		public string IssueTarget { get; set; }

		[Required, MinLength(20), MaxLength(10000)]
		public string ProblemDescription { get; set; }
	}

	public enum TechnicalType 
	{
		Unknown,
		Other,
		DiscordServer,
		DiscordBot,
		Website
	}
}
