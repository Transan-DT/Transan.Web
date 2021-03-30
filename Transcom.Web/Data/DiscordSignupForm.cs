using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Transcom.Web.Data
{
	public record DiscordSignupForm
	{
		public const int MaxContentLength = 10000; 

		/// <summary>
		/// Discord Snowflake (ID)
		/// </summary>
		[BsonId, BsonRepresentation(BsonType.String)]
		public ulong UserSnowflake { get; init; }

		/// <summary>
		/// Date & Time of form submission
		/// </summary>
		public DateTime SubmittedAt { get; init; }

		/// <summary>
		/// IP address from User
		/// </summary>
		public string IpAddress { get; init; }
		
		/// <summary>
		/// User's Gender Orientation
		/// </summary>
		public Orientation Orientation { get; set; }

		/// <summary>
		/// User's Presentation Text
		/// </summary>
		[Required, StringLength(MaxContentLength, MinimumLength = 50)]
		public string Presentation { get; set; }

		/// <summary>
		/// User's Motivation to join the server
		/// </summary>
		[Required, StringLength(MaxContentLength, MinimumLength = 50)]
		public string Motivation { get; set; }

		/// <summary>
		/// User's own Definition of their selected <see cref="Orientation"/>
		/// </summary>
		/// <remarks>
		/// Unavailable to <see cref="Orientation.Cisgenders"/>.
		/// For <see cref="Orientation.Questioning"/> users, this equates to their present feelings and thoughts.
		/// </remarks>
		[Required, StringLength(10000, MinimumLength = 50)]
		public string OrientationDefinition { get; set; }

		/// <summary>
		/// Current server member who referred this applying user.
		/// </summary>
		/// <remarks>
		/// Used for Cisgender signups only.
		/// </remarks>
		[MaxLength(106)]
		public string ReferalUser { get; set; }
	}

	public enum Orientation
	{
		Unknown,
		TransgenderMale,
		TransgenderFemale,
		NonBinary,
		GenderFluid,
		Cisgender,
		Questioning,
		Other
	}
}
