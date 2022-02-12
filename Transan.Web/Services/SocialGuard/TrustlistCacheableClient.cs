using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Hubs;
using SocialGuard.Common.Services;

namespace Transan.Web.Services.SocialGuard;

public class TrustlistCacheableClient : TrustlistClient, IHostedService, IAsyncDisposable
{
	public static Uri TrustlistHubUri { get; } = new(new(MainHost), "/hubs/trustlist");
	
	private readonly IMemoryCache _cache;
	private readonly ILogger _logger;
	private HubConnection _hub;

	public TrustlistCacheableClient(HttpClient httpClient, IMemoryCache cache, ILogger<TrustlistCacheableClient> logger) : base(httpClient)
	{
		_cache = cache;
		_logger = logger;
	}
	
	public async Task StartAsync(CancellationToken ct)
	{
		try
		{
			_hub = new HubConnectionBuilder()
				.WithUrl(TrustlistHubUri)
				.AddMessagePackProtocol()
				.WithAutomaticReconnect()
				.Build();

			_hub.On<ulong, TrustlistEntry, byte>(nameof(ITrustlistHubPush.NotifyNewEntry), (userId, _, _) => _cache.Remove(userId));
			_hub.On<ulong, TrustlistEntry, byte>(nameof(ITrustlistHubPush.NotifyEscalatedEntry), (userId, _, _) => _cache.Remove(userId));
			_hub.On<ulong, TrustlistEntry, byte>(nameof(ITrustlistHubPush.NotifyDeletedEntry), (userId, _, _) => _cache.Remove(userId));
			
			await _hub.StartAsync(ct);

			_logger.LogInformation(
				"Hooked SocialGuard Trustlist cache to Hub (State: {State}, Connection ID: {ConnectionId}).",
				_hub.State.ToString(), _hub.ConnectionId
			);
		}
		catch
		{
			_logger.LogWarning("Trustlist hub not available at address {Url}.", TrustlistHubUri);
			_hub = null;
		}
	}

	public async Task StopAsync(CancellationToken ct)
	{
		await _hub.StopAsync(ct);
	}

	public new Task<TrustlistUser?> LookupUserAsync(ulong userId) => LookupUserAsync(userId, CancellationToken.None);
	public new async Task<TrustlistUser?> LookupUserAsync(ulong userId, CancellationToken ct)
	{
		if (!_cache.TryGetValue(userId, out TrustlistUser? record))
		{
			// Cache was not hit.
			try
			{
				record = await base.LookupUserAsync(userId, ct);
				_cache.Set(userId, record);
			}
			catch
			{
				_cache.Remove(userId);

				throw;
			}
		}
		
		return record;
	}

	public new Task<TrustlistUser[]> LookupUsersAsync(ulong[] userIds) => LookupUsersAsync(userIds, CancellationToken.None);

	public new async Task<TrustlistUser[]> LookupUsersAsync(ulong[] userIds, CancellationToken ct)
	{
		List<TrustlistUser> records = new();
		List<ulong> remainingIds = new();
		
		foreach (ulong id in userIds)
		{
			if (_cache.TryGetValue(id, out TrustlistUser existing))
			{
				if (existing is not null)
				{
					records.Add(existing);
				}
			}
			else
			{
				remainingIds.Add(id);
			}
		}

		// Only remains the not cached.
		
		if (remainingIds.Any())
		{
			records.AddRange(await base.LookupUsersAsync(remainingIds.ToArray(), ct));
			
			foreach (ulong id in remainingIds)
			{
				try
				{
					_cache.Set(id, records.FirstOrDefault(u => u.Id == id));
				}
				catch
				{
					_cache.Remove(id);
					throw;
				}
			}
		}

		return records.ToArray();
	}

	public async ValueTask DisposeAsync()
	{
		ValueTask hubDisposal = _hub.DisposeAsync();
		
		_cache?.Dispose();
		await hubDisposal;
		GC.SuppressFinalize(this);
	}


}