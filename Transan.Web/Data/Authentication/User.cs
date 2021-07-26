using MongoDB.Bson.Serialization.Attributes;
using System.Security.Claims;



namespace Transan.Web.Data.Authentication
{
	public record User
	{
		[BsonId] 
		public string Snowflake { get; init; }
		
		public Claim[] Claims { get; set; }
	}
}
