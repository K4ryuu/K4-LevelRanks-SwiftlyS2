using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace K4Ranks.Stats.Events;

// =========================================
// =           PLAYER DEATH HANDLER
// =========================================

/// <summary>
/// Handles all EventPlayerDeath processing in a single hook.
/// Includes: Kill, Death, Headshot, Assist, TeamKill, Suicide, Killstreak, Special Kills
/// </summary>
public sealed class PlayerDeathHandler(PluginConfig config, PointsConfig points, ModuleConfig modules, Func<IPlayer, PlayerData?> getPlayerData, Action<IPlayer, int, string, bool, string?> modifyPoints, Func<bool> canProcess)
{
	// =========================================
	// =           MAIN HANDLER
	// =========================================

	public HookResult OnPlayerDeath(EventPlayerDeath @event)
	{
		if (!canProcess())
			return HookResult.Continue;

		var attacker = @event.Accessor.GetPlayer("attacker");
		var victim = @event.UserIdPlayer;
		var assister = @event.Accessor.GetPlayer("assister");

		var attackerData = IsValidPlayer(attacker) ? getPlayerData(attacker) : null;
		var victimData = IsValidPlayer(victim) ? getPlayerData(victim) : null;
		var assisterData = IsValidPlayer(assister) ? getPlayerData(assister) : null;

		var weapon = @event.Weapon ?? "";
		var headshot = @event.Headshot;

		// Determine kill type
		var isSuicide = !attacker.IsValid || attacker.SteamID == victim.SteamID;
		var isTeamKill = !isSuicide && !config.Rank.FFAMode &&
			attacker.IsValid && victim.IsValid &&
			attacker.Controller?.Team == victim.Controller?.Team;
		var isBotKill = victim.IsFakeClient;
		var isValidKill = !isSuicide && !isTeamKill && (!isBotKill || config.Rank.PointsForBots);

		// =========================================
		// =           VICTIM PROCESSING
		// =========================================

		if (victimData?.IsLoaded == true)
		{
			if (isSuicide)
			{
				// Suicide
				ProcessPoints(victim, points.Suicide, "k4.reason.suicide");
			}
			else if (!isTeamKill && attacker.IsValid)
			{
				// Normal death
				var multiplier = CalculateDynamicMultiplier(victimData.Points, attackerData?.Points ?? 0);
				var deathPoints = (int)Math.Round(points.Death * multiplier);
				var attackerName = attacker.Controller?.PlayerName;

				ProcessPoints(victim, deathPoints, "k4.reason.death", attackerName);
				victimData.Deaths++;

				if (modules.WeaponStatsEnabled && !string.IsNullOrEmpty(weapon))
					victimData.RecordWeaponDeath(weapon);
			}
		}

		// =========================================
		// =           ATTACKER PROCESSING
		// =========================================

		if (attackerData?.IsLoaded == true && attacker.IsValid && !isSuicide)
		{
			var victimName = victim.Controller?.PlayerName;

			if (isTeamKill)
			{
				// Team kill penalty
				ProcessPoints(attacker, points.TeamKill, "k4.reason.teamkill", victimName);
			}
			else if (isValidKill)
			{
				// Normal kill
				var multiplier = CalculateDynamicMultiplier(attackerData.Points, victimData?.Points ?? 0);
				var killPoints = (int)Math.Round(points.Kill * multiplier);

				ProcessPoints(attacker, killPoints, "k4.reason.kill", victimName);
				attackerData.Kills++;

				if (modules.WeaponStatsEnabled && !string.IsNullOrEmpty(weapon))
					attackerData.RecordWeaponKill(weapon, headshot);

				// Headshot bonus
				if (headshot)
				{
					ProcessPoints(attacker, points.Headshot, "k4.reason.headshot", victimName);
					attackerData.Headshots++;
				}

				// Special kill bonuses
				ProcessSpecialKills(@event, attacker, victimName);

				// Killstreak
				ProcessKillstreak(attacker, attackerData);
			}
		}

		// =========================================
		// =           ASSISTER PROCESSING
		// =========================================

		if (assisterData?.IsLoaded == true && assister.IsValid)
		{
			var isTeamAssist = !config.Rank.FFAMode && victim.IsValid &&
				assister.Controller?.Team == victim.Controller?.Team;

			if (@event.AssistedFlash)
			{
				// Flash assist
				if (isTeamAssist)
					ProcessPoints(assister, points.TeamAssistFlash, "k4.reason.team_assist_flash");
				else
					ProcessPoints(assister, points.AssistFlash, "k4.reason.assist_flash");
			}
			else
			{
				// Normal assist
				if (isTeamAssist)
					ProcessPoints(assister, points.TeamAssist, "k4.reason.team_assist");
				else
				{
					ProcessPoints(assister, points.Assist, "k4.reason.assist");
					assisterData.Assists++;
				}
			}
		}

		return HookResult.Continue;
	}

	// =========================================
	// =           SPECIAL KILLS
	// =========================================

	private void ProcessSpecialKills(EventPlayerDeath @event, IPlayer attacker, string? victimName)
	{
		// Early return if no special kill bonuses are configured
		if (points.NoScope == 0 && points.Thrusmoke == 0 && points.BlindKill == 0 &&
			points.Penetrated == 0 && points.LongDistanceKill == 0 && points.KnifeKill == 0 &&
			points.TaserKill == 0 && points.GrenadeKill == 0 && points.InfernoKill == 0 && points.ImpactKill == 0)
			return;

		// No-scope
		if (@event.NoScope && points.NoScope != 0)
			ProcessPoints(attacker, points.NoScope, "k4.reason.noscope", victimName);

		// Through smoke
		if (@event.ThruSmoke && points.Thrusmoke != 0)
			ProcessPoints(attacker, points.Thrusmoke, "k4.reason.thrusmoke", victimName);

		// Blind kill
		if (@event.AttackerBlind && points.BlindKill != 0)
			ProcessPoints(attacker, points.BlindKill, "k4.reason.blind", victimName);

		// Wallbang
		if (@event.Penetrated > 0 && points.Penetrated != 0)
			ProcessPoints(attacker, points.Penetrated, "k4.reason.penetrated", victimName);

		// Long distance
		if (@event.Distance >= points.LongDistance && points.LongDistanceKill != 0)
			ProcessPoints(attacker, points.LongDistanceKill, "k4.reason.longdistance", victimName);

		// Weapon type bonuses - only process if any weapon bonus is configured
		if (points.KnifeKill != 0 || points.TaserKill != 0 || points.GrenadeKill != 0 ||
			points.InfernoKill != 0 || points.ImpactKill != 0)
		{
			var weapon = @event.Weapon?.ToLowerInvariant() ?? "";
			var weaponType = Plugin.WeaponCache.GetWeaponType(@event.Weapon);

			if (weaponType == CSWeaponType.WEAPONTYPE_KNIFE && points.KnifeKill != 0)
				ProcessPoints(attacker, points.KnifeKill, "k4.reason.knife", victimName);

			if (weaponType == CSWeaponType.WEAPONTYPE_TASER && points.TaserKill != 0)
				ProcessPoints(attacker, points.TaserKill, "k4.reason.taser", victimName);

			if (weapon.Contains("hegrenade") && points.GrenadeKill != 0)
				ProcessPoints(attacker, points.GrenadeKill, "k4.reason.grenade", victimName);

			if ((weapon.Contains("inferno") || weapon.Contains("molotov") || weapon.Contains("incgrenade")) && points.InfernoKill != 0)
				ProcessPoints(attacker, points.InfernoKill, "k4.reason.inferno", victimName);

			if ((weapon.Contains("flashbang") || weapon.Contains("smokegrenade") || weapon.Contains("decoy")) && points.ImpactKill != 0)
				ProcessPoints(attacker, points.ImpactKill, "k4.reason.impact", victimName);
		}
	}

	// =========================================
	// =           KILLSTREAK
	// =========================================

	private void ProcessKillstreak(IPlayer attacker, PlayerData data)
	{
		// Early return if no killstreak bonuses are configured
		if (points.DoubleKill == 0 && points.TripleKill == 0 && points.Domination == 0 &&
			points.Rampage == 0 && points.MegaKill == 0 && points.Ownage == 0 &&
			points.UltraKill == 0 && points.KillingSpree == 0 && points.MonsterKill == 0 &&
			points.Unstoppable == 0 && points.GodLike == 0)
			return;

		var now = DateTime.UtcNow;

		if (points.SecondsBetweenKills > 0 && (now - data.LastKillTime).TotalSeconds > points.SecondsBetweenKills)
			data.Killstreak = 1;
		else
			data.Killstreak++;

		data.LastKillTime = now;

		var (_points, key) = data.Killstreak switch
		{
			2 => (points.DoubleKill, "k4.reason.doublekill"),
			3 => (points.TripleKill, "k4.reason.triplekill"),
			4 => (points.Domination, "k4.reason.domination"),
			5 => (points.Rampage, "k4.reason.rampage"),
			6 => (points.MegaKill, "k4.reason.megakill"),
			7 => (points.Ownage, "k4.reason.ownage"),
			8 => (points.UltraKill, "k4.reason.ultrakill"),
			9 => (points.KillingSpree, "k4.reason.killingspree"),
			10 => (points.MonsterKill, "k4.reason.monsterkill"),
			11 => (points.Unstoppable, "k4.reason.unstoppable"),
			>= 12 => (points.GodLike, "k4.reason.godlike"),
			_ => (0, "")
		};

		if (_points != 0 && !string.IsNullOrEmpty(key))
			ProcessPoints(attacker, _points, key);
	}

	// =========================================
	// =           HELPERS
	// =========================================

	private void ProcessPoints(IPlayer player, int points, string key, string? info = null)
	{
		if (points == 0)
			return;

		modifyPoints(player, points, key, true, info);
	}

	private static bool IsValidPlayer(IPlayer? player) =>
		player?.IsValid == true && !player.IsFakeClient;

	private double CalculateDynamicMultiplier(int playerPoints, int otherPoints)
	{
		if (!config.Points.DynamicDeathPoints || otherPoints <= 0)
			return 1.0;

		var ratio = (double)otherPoints / Math.Max(playerPoints, 1);
		return Math.Clamp(ratio, config.Points.DynamicDeathPointsMin, config.Points.DynamicDeathPointsMax);
	}
}
