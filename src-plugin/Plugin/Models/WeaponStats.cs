using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dommel;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace K4Ranks;

// =========================================
// =           WEAPON STAT MODEL
// =========================================

/// <summary>
/// Per-weapon statistics for a player
/// </summary>
public sealed class WeaponStat
{
	public required string WeaponClassname { get; init; }
	public CSWeaponType WeaponType { get; set; }

	public int Kills { get; set; }
	public int Deaths { get; set; }
	public int Headshots { get; set; }
	public long Hits { get; set; }
	public long Shots { get; set; }
	public long Damage { get; set; }

	public bool IsDirty { get; set; }
}

// =========================================
// =           WEAPON STATS COLLECTION
// =========================================

/// <summary>
/// Collection of weapon stats for a player (runtime)
/// </summary>
public sealed class PlayerWeaponStats
{
	private readonly Dictionary<string, WeaponStat> _stats = new(StringComparer.OrdinalIgnoreCase);

	// =========================================
	// =           ACCESSORS
	// =========================================

	/// <summary>Get or create weapon stat entry</summary>
	public WeaponStat GetOrCreate(string weaponClassname)
	{
		var key = NormalizeWeaponName(weaponClassname);

		if (!_stats.TryGetValue(key, out var stat))
		{
			var weaponInfo = Plugin.WeaponCache.GetByClassname(key);
			stat = new WeaponStat
			{
				WeaponClassname = key,
				WeaponType = weaponInfo?.Type ?? CSWeaponType.WEAPONTYPE_UNKNOWN
			};
			_stats[key] = stat;
		}

		return stat;
	}

	/// <summary>Get weapon stat if exists</summary>
	public WeaponStat? Get(string weaponClassname)
	{
		var key = NormalizeWeaponName(weaponClassname);
		return _stats.TryGetValue(key, out var stat) ? stat : null;
	}

	/// <summary>Get all weapon stats</summary>
	public IReadOnlyCollection<WeaponStat> GetAll() => _stats.Values;

	/// <summary>Get dirty (modified) stats</summary>
	public IEnumerable<WeaponStat> GetDirty() => _stats.Values.Where(s => s.IsDirty);

	// =========================================
	// =           MANAGEMENT
	// =========================================

	/// <summary>Clear all stats</summary>
	public void Clear() => _stats.Clear();

	/// <summary>Load from database records</summary>
	public void LoadFrom(IEnumerable<WeaponStatRecord> records)
	{
		_stats.Clear();

		foreach (var record in records)
		{
			var weaponInfo = Plugin.WeaponCache.GetByClassname(record.Classname);
			var stat = new WeaponStat
			{
				WeaponClassname = record.Classname,
				WeaponType = weaponInfo?.Type ?? CSWeaponType.WEAPONTYPE_UNKNOWN,
				Kills = record.Kills,
				Deaths = record.Deaths,
				Headshots = record.Headshots,
				Hits = record.Hits,
				Shots = record.Shots,
				Damage = record.Damage,
				IsDirty = false
			};

			_stats[record.Classname] = stat;
		}
	}

	// =========================================
	// =           HELPERS
	// =========================================

	private static string NormalizeWeaponName(string name)
	{
		var normalized = name.ToLowerInvariant();
		return normalized.StartsWith("weapon_") ? normalized : $"weapon_{normalized}";
	}
}

// =========================================
// =           DATABASE RECORD
// =========================================

/// <summary>
/// Database record for weapon stats - LVL Ranks ExStats Weapons compatible
/// </summary>
[Table("lvl_base_weapons")]
public sealed class WeaponStatRecord
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	[Column("steam")]
	public string Steam { get; set; } = "";

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	[Column("classname")]
	public string Classname { get; set; } = "";

	[Column("kills")]
	public int Kills { get; set; }

	[Column("deaths")]
	public int Deaths { get; set; }

	[Column("headshots")]
	public int Headshots { get; set; }

	[Column("hits")]
	public long Hits { get; set; }

	[Column("shots")]
	public long Shots { get; set; }

	[Column("damage")]
	public long Damage { get; set; }
}
