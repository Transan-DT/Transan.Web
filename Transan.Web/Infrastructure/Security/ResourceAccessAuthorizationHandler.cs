using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Transan.Web.Services.Authentication;

namespace Transan.Web.Infrastructure.Security;

public class ResourceAccessAuthorizationHandler : AuthorizationHandler<SelfOrRoleRequirement>
{
	private readonly ILogger<ResourceAccessAuthorizationHandler> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;


	public ResourceAccessAuthorizationHandler(ILogger<ResourceAccessAuthorizationHandler> logger, IHttpContextAccessor httpContextAccessor)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
	}

	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SelfOrRoleRequirement requirement)
	{
		if (context.User.Identity is not { IsAuthenticated: true })
		{
			context.Fail(new(this, "User is not authenticated."));
		}

		ulong pathUserId;

		try
		{
			if (_httpContextAccessor.HttpContext?.Request.RouteValues is { Count: not 0 } routeValues)
			{
				if (!TryGetFirstSnowflakeFromRoute(routeValues, out pathUserId))
				{
					_logger.LogDebug("Short-circuiting authorization, there is no resource to check on.");
					context.Succeed(requirement);
					return Task.CompletedTask;
				}
			}
			
			// HttpContext is null or path empty.
			// FIXME: Blazor Server sends empty requests, forcing authz as either hard denied or bypassed on each page access.
			else
			{
				_logger.LogDebug("HttpContext is null, or route values are empty.");
				
				// context.Succeed(requirement);
				return Task.CompletedTask;
			}
		}
		catch (FormatException e)
		{
			_logger.LogDebug(e, "Error while parsing.");
			return Task.CompletedTask;
		}

		try
		{
			ulong currentUserId = Convert.ToUInt64(context.User.FindFirstValue(ClaimTypes.NameIdentifier));

			// Check for resource owner
			if (currentUserId == pathUserId)
			{
				_logger.LogDebug("User was authorized as resource owner.");
			}

			// Check for authorized roles
			else if (requirement.AllowedRoles is not null
					&& context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(r => r.Value).Intersect(requirement.AllowedRoles) is { } r
					&& r.ToArray() is { Length: not 0 } roles)
			{
				_logger.LogDebug("User was authorized with the following role(s) : {Roles}.", string.Join(", ", roles));
			}

			// Nothing matched
			else
			{
				context.Fail(new(this, "User was not authorized as either admin or resource owner."));

				return Task.CompletedTask;
			}

			context.Succeed(requirement);
		}
		catch (FormatException e)
		{
			context.Fail(new(this, $"Error parsing the UserId. \n Exception : {e}"));
		}
		catch (Exception e)
		{
			_logger.LogError(
				e,
				"Exception caught within the {Handler}. Failing authorization for {Requirement}.",
				nameof(ResourceAccessAuthorizationHandler), nameof(SelfOrRoleRequirement)
			);

			context.Fail();
		}

		return Task.CompletedTask;
	}

	private static bool TryGetFirstSnowflakeFromRoute(RouteValueDictionary routeValues, out ulong snowflake)
	{
		if (routeValues["path"]?.ToString() is { } path 
			&& Utilities.SnowflakeRegex.Match(path).Value is { } snowflakeStr
			&& ulong.TryParse(snowflakeStr, out snowflake))
		{
			return true;
		}

		snowflake = 0;
		return false;
	}
}