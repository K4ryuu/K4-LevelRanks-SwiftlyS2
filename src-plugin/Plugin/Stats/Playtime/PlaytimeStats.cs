using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

namespace K4Ranks.Stats.Playtime;

// Playtime reward system - awards points for time played
public sealed class PlaytimeReward(ISwiftlyCore core, PointsConfig points, Func<IPlayer, PlayerData?> getPlayerData, Action<IPlayer, int, string, bool, string?> modifyPoints)
{
	private DateTime _lastRewardTime = DateTime.UtcNow;

	// Called periodically to check and award playtime points
	public void CheckAndReward()
	{
		if (points.PlaytimePoints == 0 || points.PlaytimeMinutes <= 0)
			return;

		var now = DateTime.UtcNow;
		var elapsed = (now - _lastRewardTime).TotalMinutes;

		if (elapsed < points.PlaytimeMinutes)
			return;

		_lastRewardTime = now;

		foreach (var player in core.PlayerManager.GetAllPlayers())
		{
			if (!player.IsValid || player.IsFakeClient)
				continue;

			var data = getPlayerData(player);
			if (data == null || !data.IsLoaded)
				continue;

			// Skip spectators
			var team = (int)(player.Controller?.Team ?? Team.None);
			if (team <= (int)Team.Spectator)
				continue;

			modifyPoints(player, points.PlaytimePoints, "k4.reason.playtime", true, null);
		}
	}

	// Reset the timer (e.g., on map change)
	public void Reset()
	{
		_lastRewardTime = DateTime.UtcNow;
	}
}
