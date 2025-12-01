using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace K4Ranks.Stats.Events;

// =========================================
// =           HOSTAGE EVENTS HANDLER
// =========================================

/// <summary>
/// Handles all hostage-related events in separate hooks.
/// Includes: Rescue, RescueAll, Hurt, Kill
/// </summary>
public sealed class HostageEventsHandler(ISwiftlyCore core, PointsConfig points, Func<IPlayer, PlayerData?> getPlayerData, Action<IPlayer, int, string, bool, string?> modifyPoints, Func<bool> canProcess)
{
	// =========================================
	// =           HOSTAGE RESCUED
	// =========================================

	public HookResult OnHostageRescued(EventHostageRescued @event)
	{
		if (!canProcess() || points.HostageRescue == 0)
			return HookResult.Continue;

		var player = @event.UserIdPlayer;
		if (!IsValidLoaded(player))
			return HookResult.Continue;

		modifyPoints(player, points.HostageRescue, "k4.reason.hostagerescue", true, null);
		return HookResult.Continue;
	}

	// =========================================
	// =           ALL HOSTAGES RESCUED
	// =========================================

	public HookResult OnHostageRescuedAll(EventHostageRescuedAll @event)
	{
		if (!canProcess() || points.HostageRescueAll == 0)
			return HookResult.Continue;

		foreach (var player in core.PlayerManager.GetAllPlayers())
		{
			if (!IsValidLoaded(player))
				continue;

			// Team 3 = CounterTerrorist
			var team = (int)(player.Controller?.Team ?? Team.None);
			if (team != 3)
				continue;

			modifyPoints(player, points.HostageRescueAll, "k4.reason.hostagerescue_all", true, null);
		}

		return HookResult.Continue;
	}

	// =========================================
	// =           HOSTAGE HURT
	// =========================================

	public HookResult OnHostageHurt(EventHostageHurt @event)
	{
		if (!canProcess() || points.HostageHurt == 0)
			return HookResult.Continue;

		var player = @event.UserIdPlayer;
		if (!IsValidLoaded(player))
			return HookResult.Continue;

		modifyPoints(player, points.HostageHurt, "k4.reason.hostagehurt", true, null);
		return HookResult.Continue;
	}

	// =========================================
	// =           HOSTAGE KILLED
	// =========================================

	public HookResult OnHostageKilled(EventHostageKilled @event)
	{
		if (!canProcess() || points.HostageKill == 0)
			return HookResult.Continue;

		var player = @event.UserIdPlayer;
		if (!IsValidLoaded(player))
			return HookResult.Continue;

		modifyPoints(player, points.HostageKill, "k4.reason.hostagekill", true, null);
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
