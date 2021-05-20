using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Transcom.Web.Services.Authentication;

namespace Transcom.Web.Services
{
	public class SignupControlService
	{
		internal DiscordGuild Guild { get; private set; }

		private readonly DiscordClient client;
		private readonly IConfiguration config;

		public SignupControlService(DiscordClient client, IConfiguration config)
		{
			this.client = client;
			this.config = config;

			Guild = client.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id")).GetAwaiter().GetResult();
		}

		public DiscordMember ResolveMember(ulong snowflake) => Guild.Members.TryGetValue(snowflake, out DiscordMember member) ? member : null;

		public IEnumerable<string> GetMemberRoles(DiscordMember member)
		{
			Dictionary<string, ulong> roles = new();
			config.GetSection("DiscordIntegration:Server:Roles").Bind(roles);

			foreach (string role in roles.Join(member.Roles, r1 => r1.Value, r2 => r2.Id, (r1, r2) => r1.Key))
			{
				yield return role;
			}
		}
	}
}
