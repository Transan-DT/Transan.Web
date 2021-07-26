using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Transan.Web.Data.Authentication;



namespace Transan.Web.Services.Authentication
{
	public class AuthService
	{
		private readonly IMongoCollection<User> users;

		public AuthService(IMongoClient client, IConfiguration configuration)
		{
			IMongoDatabase db = client.GetDatabase(configuration["MongoDb:Databases:Auth"]);
			users = db.GetCollection<User>("Users");
			ProvisionDb();
		}

		public async Task<User> FetchUserAsync(string snowflake) => (await users.FindAsync(u => u.Snowflake == snowflake)).FirstOrDefault();

		public async Task<bool> UserHasClaimAsync(string snowflake, Claim claim) => await FetchUserAsync(snowflake) is not null and User user && user.Claims.Contains(claim);

		private void ProvisionDb()
		{
			if (users.EstimatedDocumentCount() is 0)
			{
				users.InsertOne(new User() with { Snowflake = "278265197280362496", Claims = new Claim[] { new(ClaimTypes.Role, UserRoles.Admin) } });
			}
		}
	}
}
