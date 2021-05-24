using Microsoft.AspNetCore.Components;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Transcom.Web.Data
{
	public record GlossaryEntry
	{
		[BsonId]
		public ObjectId Id { get; init; }
		
		[Required, RegularExpression("^[a-z0-9-_]+$"), MaxLength(30)]
		public string UrlTitle { get; set; }

		[Required, MaxLength(60)]
		public string DisplayTitle { get; set; }

		public string Content { get; set; }

		public bool Visible { get; set; }


		public MarkupString Markup => new(Content);
	}
}
