using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dommel;
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
		// =           LOAD OPERATIONS
		// =========================================

		public async Task<PlayerSettings?> LoadPlayerSettingsAsync(string visibleSteamId)
		{
			if (!IsEnabled)
				return null;

			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				return await connection.GetAsync<PlayerSettings>(visibleSteamId);
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
				settings.Steam = visibleSteamId;

				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				var existing = await connection.GetAsync<PlayerSettings>(visibleSteamId);
				if (existing != null)
				{
					await connection.UpdateAsync(settings);
				}
				else
				{
					await connection.InsertAsync(settings);
				}

				settings.IsDirty = false;
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
				using var connection = Core.Database.GetConnection(_connectionName);
				connection.Open();

				foreach (var (steam, settings) in dirty)
				{
					settings.Steam = steam;
					var existing = await connection.GetAsync<PlayerSettings>(steam);
					if (existing != null)
					{
						await connection.UpdateAsync(settings);
					}
					else
					{
						await connection.InsertAsync(settings);
					}
					settings.IsDirty = false;
				}
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
[Table("lvl_base_settings")]
public sealed class PlayerSettings
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	[Column("steam")]
	public string Steam { get; set; } = "";

	[Column("messages")]
	public bool Messages { get; set; } = true;

	[Column("summary")]
	public bool Summary { get; set; } = false;

	[Column("rankchanges")]
	public bool RankChanges { get; set; } = true;

	[Ignore]
	public bool IsDirty { get; set; } = false;
}
