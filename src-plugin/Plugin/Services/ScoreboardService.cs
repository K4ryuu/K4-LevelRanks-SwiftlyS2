using System.Collections.Concurrent;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace K4Ranks;

/// <summary>
/// Handles scoreboard rank display and reveal all functionality
/// </summary>
public sealed class ScoreboardService(Plugin plugin)
{
	/* ==================== Fields ==================== */

	private readonly ISwiftlyCore _core = Plugin.Core;
	private bool _isRunning;

	/// <summary>
	/// Per-player virtual point overrides.
	/// When set, both the displayed rank icon and the displayed point value are
	/// derived from this virtual points figure rather than the player's real points.
	/// The actual <see cref="PlayerData.Points"/> value is never modified.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, int> _pointOverrides = new();

	/// <summary>
	/// Cached scoreboard values computed on <c>round_prestart</c> (or immediately
	/// when an override is set). The tick handler reads this cache and writes it to
	/// the controller every frame so the rank icon stays visible continuously.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, (int RankId, int DisplayPoints)> _cachedRanks = new();

	/* ==================== Rank Overrides ==================== */

	/// <summary>
	/// Virtually overrides the scoreboard-displayed rank and point value for a player.
	/// Both the rank icon and the Premier-mode point number shown in the CS2 scoreboard
	/// are derived from <paramref name="virtualPoints"/>. The player's actual
	/// <see cref="PlayerData.Points"/> is never modified.
	/// </summary>
	/// <param name="steamId">64-bit Steam ID of the target player.</param>
	/// <param name="virtualPoints">Virtual point value to display on the scoreboard.</param>
	public void SetPointOverride(ulong steamId, int virtualPoints) =>
		_pointOverrides[steamId] = virtualPoints;

	/// <summary>
	/// Removes a previously set virtual point override so the scoreboard reverts to
	/// displaying the rank computed from the player's real points.
	/// </summary>
	public void ClearPointOverride(ulong steamId) =>
		_pointOverrides.TryRemove(steamId, out _);

	/// <summary>
	/// Returns whether a virtual point override is active for the given player, and
	/// the override value if so.
	/// </summary>
	public bool TryGetPointOverride(ulong steamId, out int virtualPoints) =>
		_pointOverrides.TryGetValue(steamId, out virtualPoints);

	/// <summary>
	/// Removes the cached rank entry for a player. Call on disconnect to prevent
	/// stale data accumulating in <see cref="_cachedRanks"/>.
	/// </summary>
	public void RemoveCachedRank(ulong steamId)
	{
		_cachedRanks.TryRemove(steamId, out _);
		_pointOverrides.TryRemove(steamId, out _);
	}

	/* ==================== Per-Player Refresh ==================== */

	/// <summary>
	/// Recalculates and caches the effective scoreboard rank for a single player,
	/// then applies it immediately. Use this after setting or clearing an override
	/// so the icon updates without waiting for the next <c>round_prestart</c>.
	/// </summary>
	public void UpdatePlayerScoreboard(IPlayer player)
	{
		if (!plugin.Config.CurrentValue.Scoreboard.UseRanks)
			return;

		if (!player.IsValid || player.IsFakeClient)
			return;

		var data = plugin.PlayerData.GetPlayerData(player);
		if (data == null || !data.IsLoaded)
			return;

		var cfg = plugin.Config.CurrentValue.Scoreboard;
		CacheAndApplyScoreboardRank(player, data, cfg);
	}

	/* ==================== Start / Stop ==================== */

	public void Start()
	{
		if (_isRunning)
			return;

		_isRunning = true;

		if (plugin.Config.CurrentValue.Scoreboard.UseRanks)
			_core.Event.OnTick += ApplyAllCachedScoreboards;

		if (plugin.Config.CurrentValue.Scoreboard.RevealAllInterval > 0)
			StartRevealAllTimer();
	}

	public void Stop()
	{
		if (!_isRunning)
			return;

		_isRunning = false;
		_core.Event.OnTick -= ApplyAllCachedScoreboards;
	}

	/* ==================== Reveal All ==================== */

	private void StartRevealAllTimer()
	{
		var interval = Math.Max(plugin.Config.CurrentValue.Scoreboard.RevealAllInterval, 1f);

		_core.Scheduler.DelayBySeconds(interval, () =>
		{
			if (_isRunning)
			{
				SendRevealAll();
				StartRevealAllTimer();
			}
		});
	}

	private void SendRevealAll()
	{
		if (!_isRunning)
			return;

		try
		{
			_core.NetMessage.Send<CCSUsrMsg_ServerRankRevealAll>(msg =>
			{
				msg.Recipients.AddAllPlayers();
			});
		}
		catch
		{
			// Silently ignore if NetMessage fails
		}
	}

	/* ==================== Scoreboard Updates ==================== */

	/// <summary>
	/// Recalculates and caches the effective rank for every loaded player.
	/// Called on <c>round_prestart</c> — rank values are computed once per round
	/// and then replayed every tick by <see cref="ApplyAllCachedScoreboards"/>.
	/// </summary>
	public void UpdateAllScoreboards()
	{
		if (!plugin.Config.CurrentValue.Scoreboard.UseRanks)
			return;

		var cfg = plugin.Config.CurrentValue.Scoreboard;

		foreach (var player in _core.PlayerManager.GetAllPlayers())
		{
			if (!player.IsValid || player.IsFakeClient)
				continue;

			var data = plugin.PlayerData.GetPlayerData(player);
			if (data == null || !data.IsLoaded)
				continue;

			CacheAndApplyScoreboardRank(player, data, cfg);
		}
	}

	/// <summary>
	/// Tick handler — writes the cached rank values to every player's controller
	/// fields so the rank icon remains visible every frame.
	/// No rank recalculation happens here; that is done in
	/// <see cref="UpdateAllScoreboards"/> on <c>round_prestart</c>.
	/// </summary>
	private void ApplyAllCachedScoreboards()
	{
		if (!plugin.Config.CurrentValue.Scoreboard.UseRanks)
			return;

		var cfg = plugin.Config.CurrentValue.Scoreboard;

		foreach (var player in _core.PlayerManager.GetAllPlayers())
		{
			if (!player.IsValid || player.IsFakeClient)
				continue;

			if (!_cachedRanks.TryGetValue(player.SteamID, out var cached))
				continue;

			SetCompetitiveRank(player, cfg.RankMode, cached.RankId, cached.DisplayPoints,
				cfg.CustomRankMax, cfg.CustomRankBase, cfg.CustomRankMargin);
		}
	}

	/// <summary>
	/// Computes the effective rank (real or virtual), stores it in the cache, and
	/// applies it to the player's controller immediately.
	/// When a virtual point override is active, both the rank icon and the
	/// Premier-mode point number are derived from that override value.
	/// </summary>
	private void CacheAndApplyScoreboardRank(IPlayer player, PlayerData data, ScoreboardSettings cfg)
	{
		int effectivePoints = TryGetPointOverride(data.SteamId64, out var virtualPoints)
			? virtualPoints
			: data.Points;

		int rankId = plugin.Ranks.GetRankId(effectivePoints);

		_cachedRanks[data.SteamId64] = (rankId, effectivePoints);

		SetCompetitiveRank(player, cfg.RankMode, rankId, effectivePoints,
			cfg.CustomRankMax, cfg.CustomRankBase, cfg.CustomRankMargin);
	}

	private static void SetCompetitiveRank(
		IPlayer player,
		int mode,
		int rankId,
		int currentPoints,
		int rankMax,
		int rankBase,
		int rankMargin)
	{
		var controller = player.Controller;
		if (controller == null)
			return;

		controller.CompetitiveWins = 10;

		switch (mode)
		{
			case 1: // Premier - show points directly
				controller.CompetitiveRanking = currentPoints;
				controller.CompetitiveRankType = 11;
				break;

			case 2: // Competitive (MM ranks 1-18)
				controller.CompetitiveRanking = Math.Min(rankId, 18);
				controller.CompetitiveRankType = 12;
				break;

			case 3: // Wingman (ranks 1-18)
				controller.CompetitiveRanking = Math.Min(rankId, 18);
				controller.CompetitiveRankType = 7;
				break;

			case 4: // Danger Zone (ranks 1-15)
				controller.CompetitiveRanking = Math.Min(rankId, 15);
				controller.CompetitiveRankType = 10;
				break;

			default: // Custom mode (0)
				int calculatedRank = rankId > rankMax
					? rankBase + rankMax - rankMargin
					: rankBase + (rankId - rankMargin - 1);
				controller.CompetitiveRanking = Math.Max(0, calculatedRank);
				controller.CompetitiveRankType = 12;
				break;
		}
	}
}
