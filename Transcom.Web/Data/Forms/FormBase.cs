using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;



namespace Transcom.Web.Data.Forms
{
	public abstract record FormBase
	{
		public const int MaxContentLength = 10000;

		/// <summary>
		/// ID of Form
		/// </summary>
		[BsonId]
		public ObjectId Id { get; init; }

		/// <summary>
		/// Discord Snowflake (ID)
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		public ulong UserSnowflake { get; init; }

		/// <summary>
		/// Date & Time of Form submission
		/// </summary>
		public DateTime SubmittedAt { get; init; }

		/// <summary>
		/// IP address from User
		/// </summary>
		public string IpAddress { get; init; }
	}

	public enum ContactFormType
	{
		Unknown,
		Other,
		Technical,
		Report
	}
}
