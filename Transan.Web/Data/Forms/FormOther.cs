using System.ComponentModel.DataAnnotations;



namespace Transan.Web.Data.Forms
{
	public record FormOther : FormBase
	{
		[EmailAddress]
		public string EmailAddress { get; set; }

		public string Content { get; set; }
	}
}
