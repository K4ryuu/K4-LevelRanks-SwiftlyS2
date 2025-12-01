using Dapper;
using Microsoft.Extensions.Logging;

namespace K4Ranks;

public sealed partial class Plugin
{
	public sealed partial class DatabaseService
	{
		// =========================================
		// =           TABLE CREATION
		// =========================================

		private async Task CreateTableAsync()
		{
			const string sql = $@"
				CREATE TABLE IF NOT EXISTS `{TableName}` (
					`steam` VARCHAR(32) NOT NULL PRIMARY KEY,
					`name` VARCHAR(64) NOT NULL DEFAULT '',
					`value` INT NOT NULL DEFAULT 0,
					`rank` INT NOT NULL DEFAULT 0,
					`kills` INT NOT NULL DEFAULT 0,
					`deaths` INT NOT NULL DEFAULT 0,
					`shoots` BIGINT NOT NULL DEFAULT 0,
					`hits` BIGINT NOT NULL DEFAULT 0,
					`headshots` INT NOT NULL DEFAULT 0,
					`assists` INT NOT NULL DEFAULT 0,
					`round_win` INT NOT NULL DEFAULT 0,
					`round_lose` INT NOT NULL DEFAULT 0,
					`playtime` BIGINT NOT NULL DEFAULT 0,
					`lastconnect` INT NOT NULL DEFAULT 0,
					-- K4 Extensions --
					`game_wins` INT NOT NULL DEFAULT 0,
					`game_losses` INT NOT NULL DEFAULT 0,
					`games_played` INT NOT NULL DEFAULT 0,
					`rounds_played` INT NOT NULL DEFAULT 0,
					`damage` BIGINT NOT NULL DEFAULT 0,
					INDEX `idx_value` (`value` DESC),
					INDEX `idx_visiblerank` (`rank`),
					INDEX `idx_lastconnect` (`lastconnect`)
				) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			await connection.ExecuteAsync(sql);
		}

		// =========================================
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<PlayerData?> LoadPlayerAsync(string visibleSteamId)
		{
			if (!IsEnabled)
				return null;

			try
			{
				const string sql = $@"
					SELECT
						`steam` AS Steam,
						`name` AS Name,
						`value` AS Value,
						`rank` AS Rank,
						`kills` AS Kills,
						`deaths` AS Deaths,
						`shoots` AS Shoots,
						`hits` AS Hits,
						`headshots` AS Headshots,
						`assists` AS Assists,
						`round_win` AS RoundWin,
						`round_lose` AS RoundLose,
						`playtime` AS Playtime,
						`lastconnect` AS LastConnect,
						`game_wins` AS GameWins,
						`game_losses` AS GameLosses,
						`games_played` AS GamesPlayed,
						`rounds_played` AS RoundsPlayed,
						`damage` AS Damage
					FROM `{TableName}`
					WHERE `steam` = @Steam;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.QueryFirstOrDefaultAsync<PlayerData>(sql, new { Steam = visibleSteamId });
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
				var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				const string sql = $@"
					INSERT INTO `{TableName}` (
						`steam`, `name`, `value`, `rank`, `kills`, `deaths`, `shoots`, `hits`,
						`headshots`, `assists`, `round_win`, `round_lose`, `playtime`, `lastconnect`,
						`game_wins`, `game_losses`, `games_played`, `rounds_played`, `damage`
					) VALUES (
						@Steam, @Name, @Value, @Rank, @Kills, @Deaths, @Shoots, @Hits,
						@Headshots, @Assists, @RoundWin, @RoundLose, @Playtime, @LastConnect,
						@GameWins, @GameLosses, @GamesPlayed, @RoundsPlayed, @Damage
					) ON DUPLICATE KEY UPDATE
						`name` = @Name,
						`value` = @Value,
						`rank` = @Rank,
						`kills` = @Kills,
						`deaths` = @Deaths,
						`shoots` = @Shoots,
						`hits` = @Hits,
						`headshots` = @Headshots,
						`assists` = @Assists,
						`round_win` = @RoundWin,
						`round_lose` = @RoundLose,
						`playtime` = @Playtime,
						`lastconnect` = @LastConnect,
						`game_wins` = @GameWins,
						`game_losses` = @GameLosses,
						`games_played` = @GamesPlayed,
						`rounds_played` = @RoundsPlayed,
						`damage` = @Damage;";

				data.LastConnect = now;

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				await connection.ExecuteAsync(sql, data);
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

				const string sql = $@"
					INSERT INTO `{TableName}` (
						`steam`, `name`, `value`, `rank`, `kills`, `deaths`, `shoots`, `hits`,
						`headshots`, `assists`, `round_win`, `round_lose`, `playtime`, `lastconnect`,
						`game_wins`, `game_losses`, `games_played`, `rounds_played`, `damage`
					) VALUES (
						@Steam, @Name, @Value, @Rank, @Kills, @Deaths, @Shoots, @Hits,
						@Headshots, @Assists, @RoundWin, @RoundLose, @Playtime, @LastConnect,
						@GameWins, @GameLosses, @GamesPlayed, @RoundsPlayed, @Damage
					) ON DUPLICATE KEY UPDATE
						`name` = VALUES(`name`),
						`value` = VALUES(`value`),
						`rank` = VALUES(`rank`),
						`kills` = VALUES(`kills`),
						`deaths` = VALUES(`deaths`),
						`shoots` = VALUES(`shoots`),
						`hits` = VALUES(`hits`),
						`headshots` = VALUES(`headshots`),
						`assists` = VALUES(`assists`),
						`round_win` = VALUES(`round_win`),
						`round_lose` = VALUES(`round_lose`),
						`playtime` = VALUES(`playtime`),
						`lastconnect` = VALUES(`lastconnect`),
						`game_wins` = VALUES(`game_wins`),
						`game_losses` = VALUES(`game_losses`),
						`games_played` = VALUES(`games_played`),
						`rounds_played` = VALUES(`rounds_played`),
						`damage` = VALUES(`damage`);";

				foreach (var p in dirty)
					p.LastConnect = now;

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				await connection.ExecuteAsync(sql, dirty);

				foreach (var p in dirty)
					p.IsDirty = false;
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
				const string sql = $@"
					SELECT COUNT(*) + 1
					FROM `{TableName}`
					WHERE `value` > (SELECT `value` FROM `{TableName}` WHERE `steam` = @Steam);";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.ExecuteScalarAsync<int>(sql, new { Steam = visibleSteamId });
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
				const string sql = $"SELECT COUNT(*) FROM `{TableName}`;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.ExecuteScalarAsync<int>(sql);
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
				const string sql = $@"
					SELECT
						`steam` AS Steam,
						`name` AS Name,
						`value` AS Value,
						`rank` AS Rank,
						`kills` AS Kills,
						`deaths` AS Deaths,
						`shoots` AS Shoots,
						`hits` AS Hits,
						`headshots` AS Headshots,
						`assists` AS Assists,
						`round_win` AS RoundWin,
						`round_lose` AS RoundLose,
						`playtime` AS Playtime,
						`lastconnect` AS LastConnect,
						`game_wins` AS GameWins,
						`game_losses` AS GameLosses,
						`games_played` AS GamesPlayed,
						`rounds_played` AS RoundsPlayed,
						`damage` AS Damage
					FROM `{TableName}`
					ORDER BY `value` DESC
					LIMIT @Count;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var result = await connection.QueryAsync<PlayerData>(sql, new { Count = count });
				return [.. result];
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get top players");
				return [];
			}
		}
	}
}
