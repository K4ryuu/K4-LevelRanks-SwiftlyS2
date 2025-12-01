namespace K4Ranks;

/// <summary>
/// Module configuration (modules.json)
/// Controls which optional features are enabled/disabled.
/// Disabled modules won't create database tables or track any data.
/// </summary>
public sealed class ModuleConfig
{
	/// <summary>
	/// Weapon statistics tracking (lvl_base_weapons table)
	/// Tracks per-weapon kills, deaths, accuracy, damage
	/// </summary>
	public bool WeaponStatsEnabled { get; set; } = true;

	/// <summary>
	/// Hitbox/body part statistics tracking (lvl_base_hits table)
	/// ExStats Hits compatible - tracks damage per hitgroup
	/// </summary>
	public bool HitStatsEnabled { get; set; } = true;
}
