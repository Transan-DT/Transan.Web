using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Transan.Web.Data.RoleDeck;

namespace Transan.Web.Services;

public class RoleDeckService
{
	private readonly ILogger _logger;
	private readonly IConfiguration _config;
	private readonly DiscordClient _discordClient;
	private readonly IMongoCollection<RoleCategory> _roles;

	public RoleDeckService(ILogger<RoleDeckService> logger, IMongoClient mongoClient, IConfiguration config, DiscordClient discordClient)
	{
		_logger = logger;
		_config = config;
		_discordClient = discordClient;
		_roles = mongoClient.GetDatabase(config["MongoDb:Databases:Web"]).GetCollection<RoleCategory>("Roles");
	}

	public async Task<IEnumerable<RoleCategory>> GetRoleDeckcategoriesAsync() => 
		await (await _roles.FindAsync(FilterDefinition<RoleCategory>.Empty, 
			new() { Sort = new SortDefinitionBuilder<RoleCategory>().Descending(r => r.Order) }))
			.ToListAsync();

	public async Task<IEnumerable<DiscordRole>> GetAllServerRolesAsync(bool includeSeparatorRoles)
	{
		DiscordGuild guild = await _discordClient.GetGuildAsync(_config.GetValue<ulong>("DiscordIntegration:Server:Id"));
		return includeSeparatorRoles
			? guild.Roles.Values
			: guild.Roles.Values.Where(r => !r.Name.StartsWith('.'));
	}
	
	public async Task<IEnumerable<ulong>> GetMemberRolesAsync(ulong userId)
	{
		DiscordGuild guild = await _discordClient.GetGuildAsync(_config.GetValue<ulong>("DiscordIntegration:Server:Id"));
		return (await guild.GetMemberAsync(userId)).Roles.Select(r => r.Id);
	}

	public async Task SetMemberRoleAsync(RoleChangeDto roleChange)
	{
		DiscordGuild guild = await _discordClient.GetGuildAsync(_config.GetValue<ulong>("DiscordIntegration:Server:Id"));
		DiscordMember member = await guild.GetMemberAsync(roleChange.UserId);
		DiscordRole role = guild.GetRole(roleChange.RoleId);

		// Just a routine security check
		if (role.Permissions is not Permissions.None) // Oh? Got some perms?
		{
			// Check if role is truly and well listed in DB, as assignable.
			if (!await (await _roles.FindAsync(new FilterDefinitionBuilder<RoleCategory>().ElemMatch(r => r.Roles, r => r.Id == role.Id))).AnyAsync())
			{
				// This could've been dangerous. Imagine having assigned Admin perms to some random user in screening...
				throw new SecurityException("Attempted to assign an unlisted role with permissions attached. This has been denied as it constitutes a security risk.");
			}
		}
		
		const string reasonMessage = "[transanctuaire.fr] Assigned from user's Role Deck.";

		if (roleChange.State)
		{
			await member.GrantRoleAsync(role, reasonMessage);
		}
		else
		{
			await member.RevokeRoleAsync(role, reasonMessage);
		}
	}

	public Task CreateNewCategoryAsync(RoleCategory category) => _roles.InsertOneAsync(category with { Id = ObjectId.GenerateNewId() });

	public Task EditCategoryAsync(RoleCategory category) => _roles.FindOneAndReplaceAsync(r => r.Id == category.Id, category);

	public Task DeleteCategoryAsync(RoleCategory category) => _roles.FindOneAndDeleteAsync(c => c.Id == category.Id);
	
	/*
		FIXME: Cannot sort while inserting new Role. 
		The C# MongoDB driver doesn't have support yet for using $sort in a $push operation.
		See: https://jira.mongodb.org/browse/CSHARP-1271
	*/
	public async Task CreateRoleAsync(ObjectId categoryId, Role role)
	{
		await _roles.FindOneAndUpdateAsync(
			c => c.Id == categoryId,
			new UpdateDefinitionBuilder<RoleCategory>().Push(c => c.Roles, role)
		);
	}

	public Task EditRoleAsync(Role role) => _roles.UpdateOneAsync(
		Builders<RoleCategory>.Filter.ElemMatch(c => c.Roles, r => r.Id == role.Id),
		Builders<RoleCategory>.Update.Set(x => x.Roles[-1], role)
	);
		
	public Task DeleteRoleAsync(Role role) => _roles.UpdateOneAsync(
		Builders<RoleCategory>.Filter.ElemMatch(c => c.Roles, r => r.Id == role.Id),
		Builders<RoleCategory>.Update.Unset(x => x.Roles[-1])
	);
}