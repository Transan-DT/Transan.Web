using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Transan.Web.Data.RoleDeck;

/// <summary>
/// Represents a role within the Role Deck.
/// </summary>
public record Role
{
	/// <summary>
	/// Discord snowflake of the role.
	/// </summary>
	[BsonId, BsonRepresentation(BsonType.String)]
	public ulong Id { get; init; }
	
	/// <summary>
	/// Display name of the role.
	/// </summary>
	public string Name { get; set; } = string.Empty;
	
	/// <summary>
	/// Subtitle or alternative title associated to the role's name.
	/// </summary>
	public string? Subtitle { get; set; }
	
	/// <summary>
	/// Description of the role.
	/// </summary>
	public string? Description { get; set; }
	
	/// <summary>
	/// Icon/Image associated with the role. 
	/// </summary>
	public byte[]? Icon { get; set; }
}