using K4RanksSharedApi;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SteamAPI;

namespace K4Ranks;

public sealed partial class Plugin
{
	/// <summary>
	/// IK4RanksApi implementation for external plugins
	/// </summary>
	internal sealed class K4RanksApiService : IK4RanksApi
	{
		/* ==================== Fields ==================== */

		private readonly Plugin _plugin;
		private readonly Dictionary<string, Func<PlayerData, object>> _fieldGetters;
		private readonly Dictionary<string, Func<WeaponStat, object>> _weaponFieldGetters;

		/* ==================== Constructor ==================== */

		public K4RanksApiService(Plugin plugin)
		{
			_plugin = plugin;
			_fieldGetters = BuildFieldGetters();
			_weaponFieldGetters = BuildWeaponFieldGetters();
		}

		private Dictionary<string, Func<PlayerData, object>> BuildFieldGetters()
		{
			return new Dictionary<string, Func<PlayerData, object>>(StringComparer.OrdinalIgnoreCase)
			{
				// DB columns (lvl_base table)
				["value"] = d => d.Points,
				["points"] = d => d.Points,
				["kills"] = d => d.Kills,
				["deaths"] = d => d.Deaths,
				["shoots"] = d => d.Shoots,
				["hits"] = d => d.Hits,
				["headshots"] = d => d.Headshots,
				["assists"] = d => d.Assists,
				["round_win"] = d => d.RoundWin,
				["round_lose"] = d => d.RoundLose,
				["playtime"] = d => d.Playtime,
				["damage"] = d => d.Damage,
				["game_wins"] = d => d.GameWins,
				["game_losses"] = d => d.GameLosses,
				["games_played"] = d => d.GamesPlayed,
				["rounds_played"] = d => d.RoundsPlayed,
				["name"] = d => d.Name,
				["steam"] = d => d.Steam,

				// Computed fields
				["kdr"] = d => d.KDR,
				["accuracy"] = d => d.Accuracy,
				["hspercent"] = d => d.HeadshotPercentage,

				// Rank fields
				["rankname"] = d => _plugin.Ranks.GetRank(d.Points).Name,
				["ranktag"] = d => _plugin.Ranks.GetRank(d.Points).Tag,
				["rankcolor"] = d => _plugin.Ranks.GetRank(d.Points).Color,
				["rankid"] = d => _plugin.Ranks.GetRankId(d.Points),
			};
		}

		private static Dictionary<string, Func<WeaponStat, object>> BuildWeaponFieldGetters()
		{
			return new Dictionary<string, Func<WeaponStat, object>>(StringComparer.OrdinalIgnoreCase)
			{
				["kills"] = w => w.Kills,
				["deaths"] = w => w.Deaths,
				["headshots"] = w => w.Headshots,
				["hits"] = w => w.Hits,
				["shots"] = w => w.Shots,
				["damage"] = w => w.Damage,
			};
		}

		/* ==================== IK4RanksApi Implementation ==================== */

		public object? GetPlayerStat(IPlayer player, string field)
		{
			var data = _plugin.PlayerData.GetPlayerData(player);
			if (data == null || !data.IsLoaded)
				return null;

			// Check for weapon stat format: "weapon.ak47.kills"
			if (field.StartsWith("weapon.", StringComparison.OrdinalIgnoreCase))
				return GetWeaponStat(data, field);

			return _fieldGetters.TryGetValue(field, out var getter) ? getter(data) : null;
		}

		private object? GetWeaponStat(PlayerData data, string field)
		{
			var parts = field.Split('.');
			if (parts.Length != 3)
				return null;

			var weaponName = "weapon_" + parts[1];
			var statName = parts[2];

			var weaponStat = data.WeaponStats.Get(weaponName);
			if (weaponStat == null)
				return null;

			return _weaponFieldGetters.TryGetValue(statName, out var getter) ? getter(weaponStat) : null;
		}

		public bool IsPlayerDataLoaded(IPlayer player)
		{
			var data = _plugin.PlayerData.GetPlayerData(player);
			return data?.IsLoaded ?? false;
		}

		public void ModifyPlayerPoints(IPlayer player, int amount, string reason, bool showMessage = true)
		{
			_plugin.PlayerData.ModifyPoints(player, amount, reason, showMessage);
		}

		public void SetPlayerPoints(IPlayer player, int points)
		{
			var data = _plugin.PlayerData.GetPlayerData(player);
			if (data == null)
				return;

			data.Points = points;
			data.IsDirty = true;

			if (_plugin.Config.CurrentValue.Scoreboard.Clantags)
				_plugin.PlayerData.UpdatePlayerClanTag(player, data);
		}

		public async Task<int> GetPlayerPositionAsync(IPlayer player)
		{
			var visibleSteamId = SteamIdParser.ToSteamId(player.SteamID);
			return await _plugin.Database.GetPlayerRankPositionAsync(visibleSteamId);
		}

		public async Task<int> GetTotalPlayersAsync()
		{
			return await _plugin.Database.GetTotalPlayersAsync();
		}
	}
}
