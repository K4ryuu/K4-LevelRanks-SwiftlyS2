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

		internal const string SettingsTableName = "lvl_base_settings";

		// =========================================
		// =           TABLE CREATION
		// =========================================

		private async Task CreateSettingsTableAsync()
		{
			const string sql = $@"
				CREATE TABLE IF NOT EXISTS `{SettingsTableName}` (
					`steam` VARCHAR(32) NOT NULL PRIMARY KEY,
					`messages` TINYINT(1) NOT NULL DEFAULT 1,
					`summary` TINYINT(1) NOT NULL DEFAULT 0,
					`rankchanges` TINYINT(1) NOT NULL DEFAULT 1
				) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			await connection.ExecuteAsync(sql);
		}

		// =========================================
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<PlayerSettings?> LoadPlayerSettingsAsync(string visibleSteamId)
		{
			if (!IsEnabled)
				return null;

			try
			{
				const string sql = $@"
					SELECT
						`steam` AS Steam,
						`messages` AS Messages,
						`summary` AS Summary,
						`rankchanges` AS RankChanges
					FROM `{SettingsTableName}`
					WHERE `steam` = @Steam;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.QueryFirstOrDefaultAsync<PlayerSettings>(sql, new { Steam = visibleSteamId });
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load settings for {Steam}", visibleSteamId);
				return null;
			}
		}

		// =========================================
		// =           SAVE OPERATIONS
		// =========================================

		public async Task SavePlayerSettingsAsync(string visibleSteamId, PlayerSettings settings)
		{
			if (!IsEnabled)
				return;

			try
			{
				const string sql = $@"
					INSERT INTO `{SettingsTableName}` (`steam`, `messages`, `summary`, `rankchanges`)
					VALUES (@Steam, @Messages, @Summary, @RankChanges)
					ON DUPLICATE KEY UPDATE
						`messages` = @Messages,
						`summary` = @Summary,
						`rankchanges` = @RankChanges;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				await connection.ExecuteAsync(sql, new
				{
					Steam = visibleSteamId,
					settings.Messages,
					settings.Summary,
					settings.RankChanges
				});
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to save settings for {Steam}", visibleSteamId);
			}
		}

		public async Task SaveAllPlayerSettingsAsync(IEnumerable<(string Steam, PlayerSettings Settings)> playerSettings)
		{
			if (!IsEnabled)
				return;

			var dirty = playerSettings.Where(p => p.Settings.IsDirty).ToList();
			if (dirty.Count == 0)
				return;

			try
			{
				const string sql = $@"
					INSERT INTO `{SettingsTableName}` (`steam`, `messages`, `summary`, `rankchanges`)
					VALUES (@Steam, @Messages, @Summary, @RankChanges)
					ON DUPLICATE KEY UPDATE
						`messages` = VALUES(`messages`),
						`summary` = VALUES(`summary`),
						`rankchanges` = VALUES(`rankchanges`);";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var parameters = dirty.Select(p => new
				{
					p.Steam,
					p.Settings.Messages,
					p.Settings.Summary,
					p.Settings.RankChanges
				});

				await connection.ExecuteAsync(sql, parameters);

				foreach (var (Steam, Settings) in dirty)
					Settings.IsDirty = false;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to batch save player settings");
			}
		}
	}
}

// =========================================
// =           PLAYER SETTINGS MODEL
// =========================================

/// <summary>
/// Player settings model - separate from stats
/// </summary>
public sealed class PlayerSettings
{
	public string Steam { get; set; } = "";
	public bool Messages { get; set; } = true;
	public bool Summary { get; set; } = false;
	public bool RankChanges { get; set; } = true;
	public bool IsDirty { get; set; } = false;
}
