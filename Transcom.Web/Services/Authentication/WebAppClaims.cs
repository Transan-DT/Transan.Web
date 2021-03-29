using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Transcom.Web.Services.Authentication
{
	public class WebAppClaims : IClaimsTransformation
	{
		private readonly AuthDbService authDb;

		public WebAppClaims(AuthDbService authDb)
		{
			this.authDb = authDb;
		}

		public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
		{
			ClaimsIdentity identity = new();

			if ((await authDb.FetchUserAsync(principal.FindFirstValue(ClaimTypes.NameIdentifier)))?.Claims is Claim[] claims)
			{
				identity.AddClaims(claims);
				principal.AddIdentity(identity);
			}

			return principal;
		}
	}
}
