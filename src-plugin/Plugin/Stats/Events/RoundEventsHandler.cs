using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace K4Ranks.Stats.Events;

// =========================================
// =           ROUND EVENTS HANDLER
// =========================================

/// <summary>
/// Handles round-related events.
/// Includes: MVP, Round End (win/lose)
/// </summary>
public sealed class RoundEventsHandler(ISwiftlyCore core, PointsConfig points, Func<IPlayer, PlayerData?> getPlayerData, Action<IPlayer, int, string, bool, string?> modifyPoints, Func<bool> canProcess)
{

	// =========================================
	// =           MVP
	// =========================================

	public HookResult OnRoundMvp(EventRoundMvp @event)
	{
		if (!canProcess() || points.MVP == 0)
			return HookResult.Continue;

		var player = @event.UserIdPlayer;
		if (!IsValidLoaded(player))
			return HookResult.Continue;

		modifyPoints(player, points.MVP, "k4.reason.mvp", true, null);
		return HookResult.Continue;
	}

	// =========================================
	// =           ROUND END (Points)
	// =========================================

	public HookResult OnRoundEndPoints(EventRoundEnd @event)
	{
		if (!canProcess())
			return HookResult.Continue;

		// Early return if no round points configured (but stats still tracked below)
		var hasRoundPoints = points.RoundWin != 0 || points.RoundLose != 0;

		var winner = @event.Winner;
		if (winner <= (int)Team.Spectator)
			return HookResult.Continue;

		foreach (var player in core.PlayerManager.GetAllPlayers())
		{
			if (!player.IsValid || player.IsFakeClient)
				continue;

			var data = getPlayerData(player);
			if (data == null || !data.IsLoaded)
				continue;

			var playerTeam = (int)(player.Controller?.Team ?? Team.None);
			if (playerTeam <= (int)Team.Spectator)
				continue;

			if (playerTeam == winner)
			{
				if (hasRoundPoints && points.RoundWin != 0)
					modifyPoints(player, points.RoundWin, "k4.reason.roundwin", true, null);

				data.RoundsWon++;
				data.RoundsPlayed++;
			}
			else
			{
				if (hasRoundPoints && points.RoundLose != 0)
					modifyPoints(player, points.RoundLose, "k4.reason.roundlose", true, null);

				data.RoundsLost++;
				data.RoundsPlayed++;
			}
		}

		return HookResult.Continue;
	}

	// =========================================
	// =           HELPER
	// =========================================

	private bool IsValidLoaded(IPlayer? player)
	{
		if (player == null || !player.IsValid || player.IsFakeClient)
			return false;

		var data = getPlayerData(player);
		return data?.IsLoaded == true;
	}
}
