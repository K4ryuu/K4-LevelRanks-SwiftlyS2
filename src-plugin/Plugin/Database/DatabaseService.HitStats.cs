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

		internal const string HitsTableName = "lvl_base_hits";

		// =========================================
		// =           TABLE CREATION
		// =========================================

		private async Task CreateHitsTableAsync()
		{
			const string sql = $@"
				CREATE TABLE IF NOT EXISTS `{HitsTableName}` (
					`SteamID` VARCHAR(32) NOT NULL DEFAULT '',
					`DmgHealth` BIGINT NOT NULL DEFAULT 0,
					`DmgArmor` BIGINT NOT NULL DEFAULT 0,
					`Head` INT NOT NULL DEFAULT 0,
					`Chest` INT NOT NULL DEFAULT 0,
					`Belly` INT NOT NULL DEFAULT 0,
					`LeftArm` INT NOT NULL DEFAULT 0,
					`RightArm` INT NOT NULL DEFAULT 0,
					`LeftLeg` INT NOT NULL DEFAULT 0,
					`RightLeg` INT NOT NULL DEFAULT 0,
					`Neak` INT NOT NULL DEFAULT 0,
					PRIMARY KEY (`SteamID`)
				) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

			using var connection = Core.Database.GetConnection(_connectionName);
			connection.Open();

			await connection.ExecuteAsync(sql);
		}

		// =========================================
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<HitData?> LoadHitDataAsync(string visibleSteamId)
		{
			if (!IsEnabled || !_modules.HitStatsEnabled)
				return null;

			try
			{
				const string sql = $@"
					SELECT
						`SteamID` AS Steam,
						`DmgHealth`,
						`DmgArmor`,
						`Head`,
						`Chest`,
						`Belly`,
						`LeftArm`,
						`RightArm`,
						`LeftLeg`,
						`RightLeg`,
						`Neak`
					FROM `{HitsTableName}`
					WHERE `SteamID` = @Steam;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.QueryFirstOrDefaultAsync<HitData>(sql, new { Steam = visibleSteamId });
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load hit data for {Steam}", visibleSteamId);
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
				const string sql = $@"
					INSERT INTO `{HitsTableName}` (
						`SteamID`, `DmgHealth`, `DmgArmor`, `Head`, `Chest`, `Belly`,
						`LeftArm`, `RightArm`, `LeftLeg`, `RightLeg`, `Neak`
					) VALUES (
						@Steam, @DmgHealth, @DmgArmor, @Head, @Chest, @Belly,
						@LeftArm, @RightArm, @LeftLeg, @RightLeg, @Neak
					) ON DUPLICATE KEY UPDATE
						`DmgHealth` = @DmgHealth,
						`DmgArmor` = @DmgArmor,
						`Head` = @Head,
						`Chest` = @Chest,
						`Belly` = @Belly,
						`LeftArm` = @LeftArm,
						`RightArm` = @RightArm,
						`LeftLeg` = @LeftLeg,
						`RightLeg` = @RightLeg,
						`Neak` = @Neak;";

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				await connection.ExecuteAsync(sql, data);
				data.IsDirty = false;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to save hit data for {Steam}", data.Steam);
			}
		}
	}
}
