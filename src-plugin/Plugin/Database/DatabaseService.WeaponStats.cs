using Dapper;
using Microsoft.Extensions.Logging;

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
		// =           TABLE CREATION
		// =========================================

		private async Task CreateWeaponStatsTableAsync()
		{
			const string sql = $@"
				CREATE TABLE IF NOT EXISTS `{WeaponStatsTableName}` (
					`steam` VARCHAR(32) NOT NULL DEFAULT '',
					`classname` VARCHAR(64) NOT NULL DEFAULT '',
					`kills` INT NOT NULL DEFAULT 0,
					-- K4 Extensions --
					`deaths` INT NOT NULL DEFAULT 0,
					`headshots` INT NOT NULL DEFAULT 0,
					`hits` BIGINT NOT NULL DEFAULT 0,
					`shots` BIGINT NOT NULL DEFAULT 0,
					`damage` BIGINT NOT NULL DEFAULT 0,
					PRIMARY KEY (`steam`, `classname`)
				) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			await connection.ExecuteAsync(sql);
		}

		// =========================================
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<List<WeaponStatRecord>> LoadWeaponStatsAsync(string visibleSteamId)
		{
			if (!IsEnabled || !_modules.WeaponStatsEnabled)
				return [];

			try
			{
				const string sql = $@"
					SELECT
						`steam` AS Steam,
						`classname` AS Classname,
						`kills` AS Kills,
						`deaths` AS Deaths,
						`headshots` AS Headshots,
						`hits` AS Hits,
						`shots` AS Shots,
						`damage` AS Damage
					FROM `{WeaponStatsTableName}`
					WHERE `steam` = @Steam;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var result = await connection.QueryAsync<WeaponStatRecord>(sql, new { Steam = visibleSteamId });
				return [.. result];
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load weapon stats for {Steam}", visibleSteamId);
				return [];
			}
		}

		// =========================================
		// =           SAVE OPERATIONS
		// =========================================

		public async Task SaveWeaponStatsAsync(string visibleSteamId, IEnumerable<WeaponStat> stats)
		{
			if (!IsEnabled || !_modules.WeaponStatsEnabled)
				return;

			var dirtyStats = stats.Where(s => s.IsDirty).ToList();
			if (dirtyStats.Count == 0)
				return;

			try
			{
				const string sql = $@"
					INSERT INTO `{WeaponStatsTableName}` (
						`steam`, `classname`, `kills`, `deaths`, `headshots`, `hits`, `shots`, `damage`
					) VALUES (
						@Steam, @Classname, @Kills, @Deaths, @Headshots, @Hits, @Shots, @Damage
					) ON DUPLICATE KEY UPDATE
						`kills` = @Kills,
						`deaths` = @Deaths,
						`headshots` = @Headshots,
						`hits` = @Hits,
						`shots` = @Shots,
						`damage` = @Damage;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var records = dirtyStats.Select(s => new
				{
					Steam = visibleSteamId,
					Classname = s.WeaponClassname,
					s.Kills,
					s.Deaths,
					s.Headshots,
					s.Hits,
					s.Shots,
					s.Damage
				});

				await connection.ExecuteAsync(sql, records);

				foreach (var stat in dirtyStats)
					stat.IsDirty = false;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to save weapon stats for {Steam}", visibleSteamId);
			}
		}
	}
}
