using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SteamAPI;

namespace K4Ranks;

public sealed partial class Plugin
{
	public sealed class PlayerDataService(Plugin plugin)
	{
		/* ==================== Fields ==================== */

		private readonly Plugin _plugin = plugin;
		private readonly ConcurrentDictionary<ulong, PlayerData> _playerData = new();

		/* ==================== Public Accessors ==================== */

		public PlayerData? GetPlayerData(IPlayer player)
		{
			return _playerData.TryGetValue(player.SteamID, out var data) ? data : null;
		}

		public IEnumerable<PlayerData> GetAllLoadedPlayers()
		{
			return _playerData.Values.Where(p => p.IsLoaded);
		}

		public void RemovePlayer(ulong steamId)
		{
			_playerData.TryRemove(steamId, out _);
		}

		/* ==================== Load ==================== */

		public async Task LoadPlayerDataAsync(IPlayer player)
		{
			var steamId64 = player.SteamID;
			var visibleSteamId = SteamIdParser.ToSteamId(steamId64);

			try
			{
				var data = await LoadOrCreatePlayerData(player, steamId64, visibleSteamId);

				await LoadPlayerSettings(data, visibleSteamId);
				var weaponStatsCount = await LoadWeaponStats(data, visibleSteamId);
				await LoadHitData(data, visibleSteamId);

				_playerData[steamId64] = data;

				if (_plugin.Config.Scoreboard.Clantags)
				{
					Core.Scheduler.NextWorldUpdate(() =>
					{
						if (player.IsValid)
							UpdatePlayerClanTag(player, data);
					});
				}
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load player data for {Steam}", visibleSteamId);
			}
		}

		private async Task<PlayerData> LoadOrCreatePlayerData(IPlayer player, ulong steamId64, string visibleSteamId)
		{
			var data = await _plugin.Database.LoadPlayerAsync(visibleSteamId);

			if (data == null)
			{
				var startPoints = _plugin.Config.Rank.StartPoints;
				return new PlayerData
				{
					Steam = visibleSteamId,
					SteamId64 = steamId64,
					Name = player.Controller?.PlayerName ?? "Unknown",
					Value = startPoints,
					Rank = _plugin.Ranks.GetRankId(startPoints),
					IsLoaded = true,
					IsDirty = true
				};
			}

			data.SteamId64 = steamId64;
			data.Name = player.Controller?.PlayerName ?? data.Name;
			data.Rank = _plugin.Ranks.GetRankId(data.Points);
			data.IsLoaded = true;

			return data;
		}

		private async Task LoadPlayerSettings(PlayerData data, string visibleSteamId)
		{
			var settings = await _plugin.Database.LoadPlayerSettingsAsync(visibleSteamId);
			data.Settings = settings ?? new PlayerSettings { Steam = visibleSteamId };
		}

		private async Task<int> LoadWeaponStats(PlayerData data, string visibleSteamId)
		{
			if (!_plugin.Modules.WeaponStatsEnabled)
				return 0;

			var weaponStats = await _plugin.Database.LoadWeaponStatsAsync(visibleSteamId);
			data.WeaponStats.LoadFrom(weaponStats);

			return weaponStats.Count;
		}

		private async Task LoadHitData(PlayerData data, string visibleSteamId)
		{
			if (!_plugin.Modules.HitStatsEnabled)
				return;

			var hitData = await _plugin.Database.LoadHitDataAsync(visibleSteamId);
			data.HitData = hitData ?? new HitData { Steam = visibleSteamId };
		}

		/* ==================== Save ==================== */

		public async Task SavePlayerDataAsync(IPlayer player)
		{
			if (!_playerData.TryGetValue(player.SteamID, out var data) || !data.IsLoaded)
				return;

			data.UpdatePlaytime();

			if (data.IsDirty)
				await _plugin.Database.SavePlayerAsync(data);

			if (data.Settings.IsDirty)
				await _plugin.Database.SavePlayerSettingsAsync(data.Steam, data.Settings);

			if (_plugin.Modules.WeaponStatsEnabled && data.WeaponStatsDirty)
				await _plugin.Database.SaveWeaponStatsAsync(data.Steam, data.WeaponStats.GetAll());

			if (_plugin.Modules.HitStatsEnabled && data.HitDataDirty)
				await _plugin.Database.SaveHitDataAsync(data.HitData);
		}

		public async Task SaveAllPlayersAsync()
		{
			var loadedPlayers = _playerData.Values.Where(p => p.IsLoaded).ToList();
			if (loadedPlayers.Count == 0)
				return;

			foreach (var data in loadedPlayers)
				data.UpdatePlaytime();

			await SaveAllPlayerData(loadedPlayers);
			await SaveAllSettings(loadedPlayers);
			await SaveAllWeaponStats(loadedPlayers);
			await SaveAllHitData(loadedPlayers);
		}

		private async Task SaveAllPlayerData(List<PlayerData> loadedPlayers)
		{
			var dirtyPlayers = loadedPlayers.Where(p => p.IsDirty).ToList();
			if (dirtyPlayers.Count > 0)
				await _plugin.Database.SavePlayersAsync(dirtyPlayers);
		}

		private async Task SaveAllSettings(List<PlayerData> loadedPlayers)
		{
			var dirtySettings = loadedPlayers
				.Where(p => p.Settings.IsDirty)
				.Select(p => (p.Steam, p.Settings))
				.ToList();

			if (dirtySettings.Count > 0)
				await _plugin.Database.SaveAllPlayerSettingsAsync(dirtySettings);
		}

		private async Task SaveAllWeaponStats(List<PlayerData> loadedPlayers)
		{
			if (!_plugin.Modules.WeaponStatsEnabled)
				return;

			foreach (var data in loadedPlayers.Where(p => p.WeaponStatsDirty))
				await _plugin.Database.SaveWeaponStatsAsync(data.Steam, data.WeaponStats.GetAll());
		}

		private async Task SaveAllHitData(List<PlayerData> loadedPlayers)
		{
			if (!_plugin.Modules.HitStatsEnabled)
				return;

			foreach (var data in loadedPlayers.Where(p => p.HitDataDirty))
				await _plugin.Database.SaveHitDataAsync(data.HitData);
		}

		/* ==================== Points ==================== */

		public void ModifyPoints(IPlayer player, int amount, string reason, bool showMessage = true, string? otherPlayerName = null)
		{
			if (!_playerData.TryGetValue(player.SteamID, out var data) || !data.IsLoaded)
				return;

			if (amount == 0)
				return;

			amount = ApplyVipMultiplier(player, amount);

			var oldRank = _plugin.Ranks.GetRank(data.Points);
			data.Points += amount;
			data.RoundPoints += amount;
			data.IsDirty = true;
			var newRank = _plugin.Ranks.GetRank(data.Points);

			if (_plugin.Config.Scoreboard.Clantags && oldRank.Name != newRank.Name)
				UpdatePlayerClanTag(player, data);

			if (_plugin.Config.Scoreboard.ScoreSync)
			{
				player.Controller?.Score = data.Points;
				player.Controller?.ScoreUpdated();
			}

			if (showMessage && !_plugin.Config.Points.RoundEndSummary && data.PointMessagesEnabled)
			{
				var displayName = _plugin.Config.Points.ShowPlayerNames ? otherPlayerName : null;
				ShowPointMessage(player, amount, reason, displayName);
			}

			if (oldRank.Name != newRank.Name)
			{
				data.Rank = _plugin.Ranks.GetRankId(data.Points);
				ShowRankChangeMessage(player, oldRank, newRank, amount > 0);
			}
		}

		private int ApplyVipMultiplier(IPlayer player, int amount)
		{
			if (amount <= 0)
				return amount;

			if (_plugin.Config.Vip.Multiplier <= 1.0 || _plugin.Config.Vip.Flags.Count == 0)
				return amount;

			if (!IsVipPlayer(player.SteamID))
				return amount;

			return (int)(amount * _plugin.Config.Vip.Multiplier);
		}

		private bool IsVipPlayer(ulong steamId)
		{
			return _plugin.Config.Vip.Flags.Any(flag =>
				Core.Permission.PlayerHasPermission(steamId, flag)
			);
		}

		/* ==================== Messages ==================== */

		private static void ShowPointMessage(IPlayer player, int amount, string reasonKey, string? otherPlayerName = null)
		{
			var localizer = Core.Translation.GetPlayerLocalizer(player);
			var prefix = localizer["k4.general.prefix"];
			var reason = localizer[reasonKey];

			if (!string.IsNullOrEmpty(otherPlayerName))
				reason = $"{reason} ({otherPlayerName})";

			var messageKey = amount > 0 ? "k4.chat.points.gained" : "k4.chat.points.lost";
			player.SendChat($"{prefix} {localizer[messageKey, Math.Abs(amount), reason]}");
		}

		private static void ShowRankChangeMessage(IPlayer player, Rank oldRank, Rank newRank, bool promoted)
		{
			var localizer = Core.Translation.GetPlayerLocalizer(player);
			var prefix = localizer["k4.general.prefix"];

			var messageKey = promoted ? "k4.chat.rank.promoted" : "k4.chat.rank.demoted";
			player.SendChat($"{prefix} {localizer[messageKey, newRank.ChatColor, newRank.Name]}");
		}

		public void ShowRoundSummary(IPlayer player)
		{
			if (!_playerData.TryGetValue(player.SteamID, out var data) || !data.IsLoaded)
				return;

			if (data.RoundPoints == 0 || !data.PointMessagesEnabled)
				return;

			var localizer = Core.Translation.GetPlayerLocalizer(player);
			var prefix = localizer["k4.general.prefix"];

			var messageKey = data.RoundPoints > 0 ? "k4.chat.summary.gained" : "k4.chat.summary.lost";
			player.SendChat($"{prefix} {localizer[messageKey, Math.Abs(data.RoundPoints)]}");
		}

		/* ==================== Helpers ==================== */

		internal void UpdatePlayerClanTag(IPlayer player, PlayerData data)
		{
			var rank = _plugin.Ranks.GetRank(data.Points);
			var controller = player.Controller;

			if (controller != null)
			{
				controller.Clan = rank.Tag;
				controller.ClanUpdated();
			}
		}
	}
}
