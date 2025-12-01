using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace K4Ranks.Stats.Events;

// =========================================
// =           WEAPON FIRE HANDLER
// =========================================

/// <summary>
/// Handles all EventWeaponFire processing in a single hook.
/// Tracks: Shots fired per weapon
/// </summary>
public sealed class WeaponFireHandler(ModuleConfig modules, Func<IPlayer, PlayerData?> getPlayerData, Func<bool> canProcess)
{

	// =========================================
	// =           MAIN HANDLER
	// =========================================

	public HookResult OnWeaponFire(EventWeaponFire @event)
	{
		if (!canProcess())
			return HookResult.Continue;

		var player = @event.UserIdPlayer;

		if (!player.IsValid || player.IsFakeClient)
			return HookResult.Continue;

		var data = getPlayerData(player);
		if (data == null || !data.IsLoaded)
			return HookResult.Continue;

		var weapon = @event.Weapon;
		if (string.IsNullOrEmpty(weapon))
			return HookResult.Continue;

		// =========================================
		// =           RECORD STATS
		// =========================================

		// Global shot stats (always tracked)
		data.RecordGlobalShot();

		// Weapon-specific shot stats
		if (modules.WeaponStatsEnabled)
			data.RecordWeaponShot(weapon);

		return HookResult.Continue;
	}
}
