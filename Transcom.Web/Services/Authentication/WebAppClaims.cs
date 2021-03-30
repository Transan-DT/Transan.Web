using Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Transcom.Web.Services.Authentication
{
	public class WebAppClaims : IClaimsTransformation
	{
		private readonly AuthService authDb;
		private readonly IGuild guild;
		private readonly IConfiguration config;

		public WebAppClaims(AuthService authDb, IDiscordClient client, IConfiguration config)
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

				if (await guild?.GetUserAsync(snowflake) is not null and IGuildUser user)
				{
					identity.AddClaim(new(ClaimTypes.Role, UserRoles.Joined));

					if (user.RoleIds.Contains(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Member")))
					{
						identity.AddClaim(new(ClaimTypes.Role, UserRoles.Member));
					}
					if (user.RoleIds.Contains(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Mod")))
					{
						identity.AddClaim(new(ClaimTypes.Role, UserRoles.Moderator));
					}
					if (user.RoleIds.Contains(config.GetValue<ulong>("DiscordIntegration:Server:Roles:Admin")))
					{
						identity.AddClaim(new(ClaimTypes.Role, UserRoles.Admin));
					}
				}

				principal.AddIdentity(identity);
			}

			return principal;
		}
	}
}
