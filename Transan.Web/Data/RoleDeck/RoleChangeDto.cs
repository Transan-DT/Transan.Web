namespace Transan.Web.Data.RoleDeck;

public sealed record RoleChangeDto(ulong UserId, ulong RoleId, bool State);