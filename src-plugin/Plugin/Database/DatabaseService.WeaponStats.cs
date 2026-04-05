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

		internal const string WeaponStatsTableName = "lvl_base_weapons";

		// =========================================
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<List<WeaponStatRecord>> LoadWeaponStatsAsync(string steamId)
		{
			if (!IsEnabled || !_modules.WeaponStatsEnabled)
				return [];

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var result = await connection.SelectAsync<WeaponStatRecord>(w => w.Steam == steamId);
				return [.. result];
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load weapon stats for {Steam}", steamId);
				return [];
			}
		}

		// =========================================
		// =           SAVE OPERATIONS
		// =========================================

		public async Task SaveWeaponStatsAsync(string steamId, IEnumerable<WeaponStat> stats)
		{
			if (!IsEnabled || !_modules.WeaponStatsEnabled)
				return;

			var dirtyStats = stats.Where(s => s.IsDirty).ToList();
			if (dirtyStats.Count == 0)
				return;

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				foreach (var stat in dirtyStats)
				{
					var record = new WeaponStatRecord
					{
						Steam = steamId,
						Classname = stat.WeaponClassname,
						Kills = stat.Kills,
						Deaths = stat.Deaths,
						Headshots = stat.Headshots,
						Hits = stat.Hits,
						Shots = stat.Shots,
						Damage = stat.Damage
					};

					try
					{
						await connection.InsertAsync(record);
					}
					catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry error
					{
						await connection.UpdateAsync(record);
					}

					stat.IsDirty = false;
				}
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to save weapon stats for {Steam}", steamId);
			}
		}
	}
}
