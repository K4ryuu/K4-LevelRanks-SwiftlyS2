using Dommel;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace K4Ranks;

public sealed partial class Plugin
{
	public sealed partial class DatabaseService
	{
		// =========================================
		// =           CONSTANTS
		// =========================================

		internal const string HitsTableName = "lvl_base_hits";

		// =========================================
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<HitData?> LoadHitDataAsync(string steamId)
		{
			if (!IsEnabled || !_modules.HitStatsEnabled)
				return null;

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.GetAsync<HitData>(steamId);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load hit data for {Steam}", steamId);
				return null;
			}
		}

		// =========================================
		// =           SAVE OPERATIONS
		// =========================================

		public async Task SaveHitDataAsync(HitData data)
		{
			if (!IsEnabled || !_modules.HitStatsEnabled || !data.IsDirty)
				return;

			try
			{
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
				Core.Logger.LogError(ex, "Failed to save hit data for {Steam}", data.Steam);
			}
		}
	}
}
