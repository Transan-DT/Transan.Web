using Microsoft.AspNetCore.Authorization;

namespace Transan.Web.Infrastructure.Security;

public static class AuthorizationExtensions
{
	public static AuthorizationPolicyBuilder RequireSelfOrRole(this AuthorizationPolicyBuilder builder, params string[]? roles)
	{
		builder.AddRequirements(new SelfOrRoleRequirement(roles));
		return builder;
	}
}