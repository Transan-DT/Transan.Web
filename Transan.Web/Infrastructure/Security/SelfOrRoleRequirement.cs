using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Transan.Web.Infrastructure.Security;

public class SelfOrRoleRequirement : IAuthorizationRequirement
{
	public IReadOnlyList<string>? AllowedRoles { get; init; }

	public SelfOrRoleRequirement(params string[]? roles)
	{
		if (roles is { Length: not 0 })
		{
			AllowedRoles = roles;
		}
	}
}