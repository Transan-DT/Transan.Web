using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Transcom.Web.Services.Authentication
{
	public class WebAppClaims : IClaimsTransformation
	{
		private readonly AuthService authDb;
		private readonly DiscordGuild guild;
		private readonly IConfiguration config;

		public WebAppClaims(AuthService authDb, DiscordClient client, IConfiguration config)
		{
			this.authDb = authDb;
			this.config = config;
			guild = client.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id")).GetAwaiter().GetResult();
		}

		public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
		{
			if (principal.Identity.IsAuthenticated)
			{

				ClaimsIdentity identity = new();
				ulong snowflake = Convert.ToUInt64(principal.FindFirstValue(ClaimTypes.NameIdentifier));

				if ((await authDb.FetchUserAsync(snowflake.ToString()))?.Claims is Claim[] claims)
				{
					identity.AddClaims(claims);

				}

				if (guild is not null)
				{
					if (guild.Members.TryGetValue(snowflake, out DiscordMember member))
					{
						identity.AddClaim(new(ClaimTypes.Role, UserRoles.Joined));

						if (member.Roles.Any(r => r.Id == config.GetValue<ulong>("DiscordIntegration:Server:Roles:Member")))
						{
							identity.AddClaim(new(ClaimTypes.Role, UserRoles.Member));
						}
						if (member.Roles.Any(r => r.Id == config.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")))
						{
							identity.AddClaim(new(ClaimTypes.Role, UserRoles.Moderator));
						}
						if (member.Roles.Any(r => r.Id == config.GetValue<ulong>("DiscordIntegration:Server:Roles:Admin")))
						{
							identity.AddClaim(new(ClaimTypes.Role, UserRoles.Admin));
						}
					} 
				}

				principal.AddIdentity(identity);
			}

			return principal;
		}
	}
}
