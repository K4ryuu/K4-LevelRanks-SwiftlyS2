using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace K4Ranks.Stats.Events;

// =========================================
// =           PLAYER HURT HANDLER
// =========================================

/// <summary>
/// Handles all EventPlayerHurt processing in a single hook.
/// Tracks: Shots hit, damage dealt, hitbox distribution
/// </summary>
public sealed class PlayerHurtHandler(PluginConfig config, ModuleConfig modules, Func<IPlayer, PlayerData?> getPlayerData, Func<bool> canProcess)
{

	// =========================================
	// =           MAIN HANDLER
	// =========================================

	public HookResult OnPlayerHurt(EventPlayerHurt @event)
	{
		if (!canProcess())
			return HookResult.Continue;

		var attacker = @event.Accessor.GetPlayer("attacker");
		var victim = @event.UserIdPlayer;

		// Basic validation
		if (!attacker.IsValid || !victim.IsValid)
			return HookResult.Continue;

		if (attacker.IsFakeClient)
			return HookResult.Continue;

		if (attacker.SteamID == victim.SteamID)
			return HookResult.Continue;

		// Skip bot damage if configured
		if (!config.Rank.PointsForBots && victim.IsFakeClient)
			return HookResult.Continue;

		// Skip team damage (unless FFA)
		if (!config.Rank.FFAMode && attacker.Controller?.Team == victim.Controller?.Team)
			return HookResult.Continue;

		var attackerData = getPlayerData(attacker);
		if (attackerData == null || !attackerData.IsLoaded)
			return HookResult.Continue;

		var weapon = @event.Weapon;
		var healthDamage = @event.DmgHealth;
		var armorDamage = @event.DmgArmor;

		if (healthDamage <= 0 && armorDamage <= 0)
			return HookResult.Continue;

		// =========================================
		// =           RECORD STATS
		// =========================================

		// Global hit stats (always tracked)
		attackerData.RecordGlobalHit(healthDamage);

		// Weapon-specific hit stats
		if (modules.WeaponStatsEnabled && !string.IsNullOrEmpty(weapon))
			attackerData.RecordWeaponHit(weapon, healthDamage);

		// Hitbox data
		if (modules.HitStatsEnabled)
		{
			var hitgroup = @event.HitGroup;
			attackerData.HitData.RecordHit(hitgroup, healthDamage, armorDamage);
		}

		return HookResult.Continue;
	}
}
