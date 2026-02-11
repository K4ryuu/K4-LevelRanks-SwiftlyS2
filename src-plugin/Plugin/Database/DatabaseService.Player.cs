using Dommel;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace K4Ranks;

public sealed partial class Plugin
{
	public sealed partial class DatabaseService
	{
		// =========================================
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<PlayerData?> LoadPlayerAsync(string visibleSteamId)
		{
			if (!IsEnabled)
				return null;

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.GetAsync<PlayerData>(visibleSteamId);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load player {Steam}", visibleSteamId);
				return null;
			}
		}

		// =========================================
		// =           SAVE OPERATIONS
		// =========================================

		public async Task SavePlayerAsync(PlayerData data)
		{
			if (!IsEnabled)
				return;

			try
			{
				data.LastConnect = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				try
				{
					await connection.InsertAsync(data);
				}
				catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry error
				{
					await connection.UpdateAsync(data);
				}

				data.IsDirty = false;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to save player {Steam}", data.Steam);
			}
		}

		public async Task SavePlayersAsync(IEnumerable<PlayerData> players)
		{
			if (!IsEnabled)
				return;

			var dirty = players.Where(p => p.IsDirty && p.IsLoaded).ToList();
			if (dirty.Count == 0)
				return;

			try
			{
				var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				foreach (var data in dirty)
				{
					data.LastConnect = now;

					try
					{
						await connection.InsertAsync(data);
					}
					catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry error
					{
						await connection.UpdateAsync(data);
					}

					data.IsDirty = false;
				}
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to batch save players");
			}
		}

		// =========================================
		// =           QUERY OPERATIONS
		// =========================================

		public async Task<int> GetPlayerRankPositionAsync(string visibleSteamId)
		{
			if (!IsEnabled)
				return -1;

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				// Get player's current value
				var player = await connection.GetAsync<PlayerData>(visibleSteamId);
				if (player == null)
					return -1;

				// Count players with higher value + 1
				var higherCount = await connection.CountAsync<PlayerData>(p => p.Value > player.Value);
				return (int)higherCount + 1;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get rank position for {Steam}", visibleSteamId);
				return -1;
			}
		}

		public async Task<int> GetTotalPlayersAsync()
		{
			if (!IsEnabled)
				return 0;

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return (int)await connection.CountAsync<PlayerData>();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get total players");
				return 0;
			}
		}

		public async Task<List<PlayerData>> GetTopPlayersAsync(int count = 10)
		{
			if (!IsEnabled)
				return [];

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var allPlayers = await connection.GetAllAsync<PlayerData>();
				return allPlayers
					.OrderByDescending(p => p.Value)
					.Take(count)
					.ToList();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get top players");
				return [];
			}
		}

		public async Task<List<PlayerData>> GetTopPlayersByTimeAsync(int count = 10)
		{
			if (!IsEnabled)
				return [];

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var allPlayers = await connection.GetAllAsync<PlayerData>();
				return allPlayers
					.OrderByDescending(p => p.Playtime)
					.Take(count)
					.ToList();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get top players by time");
				return [];
			}
		}
	}
}
