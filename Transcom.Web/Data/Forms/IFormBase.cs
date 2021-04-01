using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;



namespace Transcom.Web.Data.Forms
{
	interface IFormBase
	{
		public const int MaxContentLength = 10000;

		/// <summary>
		/// ID of Form
		/// </summary>
		[BsonId]
		public ObjectId Id { get; }

		/// <summary>
		/// Discord Snowflake (ID)
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		public ulong UserSnowflake { get; }

		/// <summary>
		/// Date & Time of Form submission
		/// </summary>
		public DateTime SubmittedAt { get; }

		/// <summary>
		/// IP address from User
		/// </summary>
		public string IpAddress { get; }
	}
}
