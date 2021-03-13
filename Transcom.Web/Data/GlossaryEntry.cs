using Microsoft.AspNetCore.Components;
using MongoDB.Bson;

namespace Transcom.Web.Data
{
	public record GlossaryEntry
	{
		public ObjectId Id { get; init; }
		

		public string UrlTitle { get; init; }

		public string DisplayTitle { get; set; }

		public string Content { get; set; }


		public MarkupString Markup => new(Content);
	}
}
