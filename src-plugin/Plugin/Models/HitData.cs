namespace K4Ranks;

/// <summary>
/// Hitbox/body part damage statistics - LVL Ranks ExStats Hits compatible
/// Table: lvl_base_hits
/// </summary>
public sealed class HitData
{
	// =========================================
	// =           IDENTITY
	// =========================================

	/// <summary>Steam ID in STEAM_X:X:XXXXXX format</summary>
	public string Steam { get; set; } = "";

	// =========================================
	// =           DAMAGE STATS
	// =========================================

	/// <summary>Total health damage dealt</summary>
	public long DmgHealth { get; set; }

	/// <summary>Total armor damage dealt</summary>
	public long DmgArmor { get; set; }

	// =========================================
	// =           HITBOX COUNTS
	// =========================================

	/// <summary>Head hits count</summary>
	public int Head { get; set; }

	/// <summary>Chest hits count</summary>
	public int Chest { get; set; }

	/// <summary>Belly/stomach hits count</summary>
	public int Belly { get; set; }

	/// <summary>Left arm hits count</summary>
	public int LeftArm { get; set; }

	/// <summary>Right arm hits count</summary>
	public int RightArm { get; set; }

	/// <summary>Left leg hits count</summary>
	public int LeftLeg { get; set; }

	/// <summary>Right leg hits count</summary>
	public int RightLeg { get; set; }

	/// <summary>Neck hits count (note: LVL Ranks uses "Neak" typo)</summary>
	public int Neak { get; set; }

	// =========================================
	// =           STATE TRACKING
	// =========================================

	/// <summary>Whether data has been modified and needs saving</summary>
	public bool IsDirty { get; set; }

	// =========================================
	// =           COMPUTED PROPERTIES
	// =========================================

	/// <summary>Total hits across all body parts</summary>
	public int TotalHits => Head + Chest + Belly + LeftArm + RightArm + LeftLeg + RightLeg + Neak;

	// =========================================
	// =           METHODS
	// =========================================

	/// <summary>Record a hit to specific hitgroup</summary>
	public void RecordHit(int hitgroup, int healthDamage, int armorDamage)
	{
		DmgHealth += healthDamage;
		DmgArmor += armorDamage;

		// CS2 Hitgroup enum values
		switch (hitgroup)
		{
			case 1: // HITGROUP_HEAD
				Head++;
				break;
			case 2: // HITGROUP_CHEST
				Chest++;
				break;
			case 3: // HITGROUP_STOMACH
				Belly++;
				break;
			case 4: // HITGROUP_LEFTARM
				LeftArm++;
				break;
			case 5: // HITGROUP_RIGHTARM
				RightArm++;
				break;
			case 6: // HITGROUP_LEFTLEG
				LeftLeg++;
				break;
			case 7: // HITGROUP_RIGHTLEG
				RightLeg++;
				break;
			case 8: // HITGROUP_NECK (gear in some versions)
				Neak++;
				break;
			default:
				// HITGROUP_GENERIC (0) or unknown - count as chest
				Chest++;
				break;
		}

		IsDirty = true;
	}

	/// <summary>Reset all hit data</summary>
	public void Reset()
	{
		DmgHealth = 0;
		DmgArmor = 0;
		Head = 0;
		Chest = 0;
		Belly = 0;
		LeftArm = 0;
		RightArm = 0;
		LeftLeg = 0;
		RightLeg = 0;
		Neak = 0;
		IsDirty = true;
	}
}
