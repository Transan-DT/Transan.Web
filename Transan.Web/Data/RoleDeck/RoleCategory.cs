using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Transan.Web.Data.RoleDeck;

/// <summary>
/// Represents a Role Group or Category within the Role Deck.
/// </summary>
public record RoleCategory
{
	/// <summary>
	/// ID of the Category.
	/// </summary>
	[BsonId]
	public ObjectId Id { get; init; }
	
	/// <summary>
	/// Display Order of the category (displays from highest to lowest).
	/// </summary>
	public sbyte? Order { get; set; }

	/// <summary>
	/// Roles attached to this category.
	/// </summary>
	public virtual List<Role> Roles { get; init; } = new();

	/// <summary>
	/// Display name of the category.
	/// </summary>
	public string Name { get; set; } = string.Empty;
	
	/// <summary>
	/// Description of the category.
	/// </summary>
	public string? Description { get; set; }
	
	/// <summary>
	/// Icon/Image associated with the category. 
	/// </summary>
	public byte[]? Icon { get; set; }
}