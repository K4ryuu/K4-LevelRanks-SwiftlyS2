using Dommel;
using K4Ranks.Database.Migrations;
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
				// Run FluentMigrator migrations
				using var connection = Core.Database.GetConnection(_connectionName);
				MigrationRunner.RunMigrations(connection);

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

			Core.Logger.LogInformation("Database initialized with migrations. Tables: {Tables}", string.Join(", ", tables));
		}

		/* ==================== Maintenance ==================== */

		public async Task PurgeOldDataAsync()
		{
			if (!IsEnabled || _purgeDays <= 0)
				return;

			try
			{
				var cutoffTimestamp = (int)DateTimeOffset.UtcNow.AddDays(-_purgeDays).ToUnixTimeSeconds();

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				// Dommel doesn't support complex WHERE with AND, use Dapper for this
				var deleted = await connection.DeleteMultipleAsync<PlayerData>(
					p => p.LastConnect < cutoffTimestamp && p.LastConnect > 0);

				if (deleted > 0)
					Core.Logger.LogInformation("Purged {Count} inactive players (>{Days} days)", deleted, _purgeDays);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to purge old records");
			}
		}

		public async Task ResetPlayerAsync(string steamId)
		{
			if (!IsEnabled)
				return;

			try
			{
				await ResetPlayerStatsAsync(steamId);
				await ResetPlayerModuleDataAsync(steamId);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to reset player {Steam}", steamId);
			}
		}

		private async Task ResetPlayerStatsAsync(string steamId)
		{
			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			var player = await connection.GetAsync<PlayerData>(steamId);
			if (player == null)
				return;

			// Reset all stats
			player.Value = _startPoints;
			player.Rank = 0;
			player.Kills = 0;
			player.Deaths = 0;
			player.Shoots = 0;
			player.Hits = 0;
			player.Headshots = 0;
			player.Assists = 0;
			player.RoundWin = 0;
			player.RoundLose = 0;
			player.Playtime = 0;
			player.GameWins = 0;
			player.GameLosses = 0;
			player.GamesPlayed = 0;
			player.RoundsPlayed = 0;
			player.Damage = 0;

			await connection.UpdateAsync(player);
		}

		private async Task ResetPlayerModuleDataAsync(string steamId)
		{
			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			if (_modules.WeaponStatsEnabled)
			{
				await connection.DeleteMultipleAsync<WeaponStatRecord>(w => w.Steam == steamId);
			}

			if (_modules.HitStatsEnabled)
			{
				var hitData = await connection.GetAsync<HitData>(steamId);
				if (hitData != null)
				{
					await connection.DeleteAsync(hitData);
				}
			}
		}
	}
}
