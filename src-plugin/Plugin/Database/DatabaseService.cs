using Dapper;
using Microsoft.Extensions.Logging;

namespace K4Ranks;

public sealed partial class Plugin
{
	/// <summary>
	/// Database service - LVL Ranks compatible structure
	/// </summary>
	public sealed partial class DatabaseService
	{
		/* ==================== Fields ==================== */

		private readonly string _connectionName;
		private readonly int _purgeDays;
		private readonly int _startPoints;
		private readonly ModuleConfig _modules;

		internal const string TableName = "lvl_base";

		/* ==================== Properties ==================== */

		public bool IsEnabled { get; private set; }

		/* ==================== Constructor ==================== */

		public DatabaseService(string connectionName, int purgeDays, int startPoints, ModuleConfig modules)
		{
			_connectionName = connectionName;
			_purgeDays = purgeDays;
			_startPoints = startPoints;
			_modules = modules;
		}

		/* ==================== Initialization ==================== */

		public async Task InitializeAsync()
		{
			try
			{
				await CreateTableAsync();
				await CreateSettingsTableAsync();

				if (_modules.WeaponStatsEnabled)
					await CreateWeaponStatsTableAsync();

				if (_modules.HitStatsEnabled)
					await CreateHitsTableAsync();

				IsEnabled = true;

				LogInitializedTables();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to initialize database");
				IsEnabled = false;
			}
		}

		private void LogInitializedTables()
		{
			var tables = new List<string> { TableName, SettingsTableName };

			if (_modules.WeaponStatsEnabled)
				tables.Add(WeaponStatsTableName);

			if (_modules.HitStatsEnabled)
				tables.Add(HitsTableName);

			Core.Logger.LogInformation("Database initialized. Tables: {Tables}", string.Join(", ", tables));
		}

		/* ==================== Maintenance ==================== */

		public async Task PurgeOldDataAsync()
		{
			if (!IsEnabled || _purgeDays <= 0)
				return;

			try
			{
				var cutoffTimestamp = (int)DateTimeOffset.UtcNow.AddDays(-_purgeDays).ToUnixTimeSeconds();

				const string sql = $@"
					DELETE FROM `{TableName}`
					WHERE `lastconnect` < @CutoffTimestamp AND `lastconnect` > 0;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var deleted = await connection.ExecuteAsync(sql, new { CutoffTimestamp = cutoffTimestamp });

				if (deleted > 0)
					Core.Logger.LogInformation("Purged {Count} inactive players (>{Days} days)", deleted, _purgeDays);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to purge old records");
			}
		}

		public async Task ResetPlayerAsync(string visibleSteamId)
		{
			if (!IsEnabled)
				return;

			try
			{
				await ResetPlayerStatsAsync(visibleSteamId);
				await ResetPlayerModuleDataAsync(visibleSteamId);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to reset player {Steam}", visibleSteamId);
			}
		}

		private async Task ResetPlayerStatsAsync(string visibleSteamId)
		{
			const string sql = $@"
				UPDATE `{TableName}`
				SET `value` = @StartPoints,
					`rank` = 0,
					`kills` = 0, `deaths` = 0, `shoots` = 0, `hits` = 0,
					`headshots` = 0, `assists` = 0,
					`round_win` = 0, `round_lose` = 0, `playtime` = 0,
					`game_wins` = 0, `game_losses` = 0, `games_played` = 0,
					`rounds_played` = 0, `damage` = 0
				WHERE `steam` = @Steam;";

			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			await connection.ExecuteAsync(sql, new { Steam = visibleSteamId, StartPoints = _startPoints });
		}

		private async Task ResetPlayerModuleDataAsync(string visibleSteamId)
		{
			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			if (_modules.WeaponStatsEnabled)
			{
				const string sql = $@"DELETE FROM `{WeaponStatsTableName}` WHERE `steam` = @Steam;";
				await connection.ExecuteAsync(sql, new { Steam = visibleSteamId });
			}

			if (_modules.HitStatsEnabled)
			{
				const string sql = $@"DELETE FROM `{HitsTableName}` WHERE `SteamID` = @Steam;";
				await connection.ExecuteAsync(sql, new { Steam = visibleSteamId });
			}
		}
	}
}
