using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Transan.Web.Services.Authentication
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
			if (principal.Identity is { IsAuthenticated: true })
			{
				ClaimsIdentity identity = new();
				ulong snowflake = Convert.ToUInt64(principal.FindFirstValue(ClaimTypes.NameIdentifier));

				if ((await authDb.FetchUserAsync(snowflake.ToString()))?.Claims is { } claims)
				{
					identity.AddClaims(claims);
				}

				try
				{
					DiscordMember member = await guild.GetMemberAsync(snowflake);

					Dictionary<string, ulong> roles = new();
					config.GetSection("DiscordIntegration:Server:Roles").Bind(roles);

					identity.AddClaim(new(ClaimTypes.Role, UserRoles.Joined));

					foreach (string role in roles.Join(member.Roles, r1 => r1.Value, r2 => r2.Id, (r1, r2) => r1.Key))
					{
						identity.AddClaim(new(ClaimTypes.Role, role));
					}
				}
				catch (NotFoundException) { }

				principal.AddIdentity(identity);
			}

			return principal;
		}
	}
}
