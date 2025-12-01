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

	/* ==================== Start / Stop ==================== */

	public void Start()
	{
		if (_isRunning)
			return;

		_isRunning = true;

		if (plugin.Config.Scoreboard.UseRanks)
			_core.Event.OnTick += UpdateScoreboards;

		if (plugin.Config.Scoreboard.RevealAllInterval > 0)
			StartRevealAllTimer();
	}

	public void Stop()
	{
		if (!_isRunning)
			return;

		_isRunning = false;
		_core.Event.OnTick -= UpdateScoreboards;
	}

	/* ==================== Reveal All ==================== */

	private void StartRevealAllTimer()
	{
		var interval = Math.Max(plugin.Config.Scoreboard.RevealAllInterval, 1f);

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

	private void UpdateScoreboards()
	{
		if (!plugin.Config.Scoreboard.UseRanks)
			return;

		var mode = plugin.Config.Scoreboard.RankMode;
		var rankMax = plugin.Config.Scoreboard.CustomRankMax;
		var rankBase = plugin.Config.Scoreboard.CustomRankBase;
		var rankMargin = plugin.Config.Scoreboard.CustomRankMargin;

		foreach (var player in _core.PlayerManager.GetAllPlayers())
		{
			if (!player.IsValid || player.IsFakeClient)
				continue;

			var data = plugin.PlayerData.GetPlayerData(player);
			if (data == null || !data.IsLoaded)
				continue;

			var rankId = plugin.Ranks.GetRankId(data.Points);
			SetCompetitiveRank(player, mode, rankId, data.Points, rankMax, rankBase, rankMargin);
		}
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
