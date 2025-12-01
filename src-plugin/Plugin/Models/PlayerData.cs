namespace K4Ranks;

/// <summary>
/// Player data model - compatible with LVL Ranks database structure
/// Table: lvl_base (main stats) + lvl_base_weapons (weapon stats)
/// </summary>
public sealed class PlayerData
{
	// =========================================
	// =    LVL RANKS COMPATIBLE FIELDS
	// =========================================

	/// <summary>Steam ID in STEAM_X:X:XXXXXX format (stored as string in DB)</summary>
	public string Steam { get; set; } = "";

	/// <summary>Player name</summary>
	public string Name { get; set; } = "";

	/// <summary>Experience/points value</summary>
	public int Value { get; set; }

	/// <summary>Current rank ID</summary>
	public int Rank { get; set; }

	/// <summary>Total kills</summary>
	public int Kills { get; set; }

	/// <summary>Total deaths</summary>
	public int Deaths { get; set; }

	/// <summary>Total shots fired (global)</summary>
	public long Shoots { get; set; }

	/// <summary>Total hits (global)</summary>
	public long Hits { get; set; }

	/// <summary>Total headshot kills</summary>
	public int Headshots { get; set; }

	/// <summary>Total assists</summary>
	public int Assists { get; set; }

	/// <summary>Rounds won</summary>
	public int RoundWin { get; set; }

	/// <summary>Rounds lost</summary>
	public int RoundLose { get; set; }

	/// <summary>Total playtime in seconds</summary>
	public long Playtime { get; set; }

	/// <summary>Last connect time (Unix timestamp)</summary>
	public int LastConnect { get; set; }

	// =========================================
	// =    EXTENDED FIELDS (K4 ADDITIONS)
	// =========================================

	/// <summary>Game/match wins</summary>
	public int GameWins { get; set; }

	/// <summary>Game/match losses</summary>
	public int GameLosses { get; set; }

	/// <summary>Total games played</summary>
	public int GamesPlayed { get; set; }

	/// <summary>Total rounds played</summary>
	public int RoundsPlayed { get; set; }

	/// <summary>Total damage dealt</summary>
	public long Damage { get; set; }

	// =========================================
	// =    PLAYER SETTINGS
	// =========================================

	/// <summary>Player settings (stored in separate table)</summary>
	public PlayerSettings Settings { get; set; } = new();

	// =========================================
	// =    RUNTIME DATA (NOT IN DB)
	// =========================================

	public ulong SteamId64 { get; set; }
	public int RoundPoints { get; set; }
	public int Killstreak { get; set; }
	public DateTime LastKillTime { get; set; } = DateTime.MinValue;
	public DateTime SessionStartTime { get; set; } = DateTime.UtcNow;
	public bool IsLoaded { get; set; }
	public bool IsDirty { get; set; }

	// =========================================
	// =    SETTINGS ALIASES
	// =========================================

	public bool PointMessagesEnabled
	{
		get => Settings.Messages;
		set { Settings.Messages = value; Settings.IsDirty = true; }
	}

	public bool ShowRoundSummary
	{
		get => Settings.Summary;
		set { Settings.Summary = value; Settings.IsDirty = true; }
	}

	public bool ShowRankChanges
	{
		get => Settings.RankChanges;
		set { Settings.RankChanges = value; Settings.IsDirty = true; }
	}

	// =========================================
	// =    WEAPON & HIT STATS
	// =========================================

	/// <summary>Weapon stats (tracked per weapon)</summary>
	public PlayerWeaponStats WeaponStats { get; } = new();
	public bool WeaponStatsDirty => WeaponStats.GetDirty().Any();

	/// <summary>Hit data (hitbox/body part tracking - ExStats Hits compatible)</summary>
	public HitData HitData { get; set; } = new();
	public bool HitDataDirty => HitData.IsDirty;

	// =========================================
	// =    COMPUTED PROPERTIES
	// =========================================

	public double KDR => Deaths == 0 ? Kills : Math.Round((double)Kills / Deaths, 2);
	public double HeadshotPercentage => Kills == 0 ? 0 : Math.Round((double)Headshots / Kills * 100, 1);
	public double Accuracy => Shoots == 0 ? 0 : Math.Round((double)Hits / Shoots * 100, 1);

	// =========================================
	// =    BACKWARDS COMPATIBILITY ALIASES
	// =========================================

	public int Points { get => Value; set => Value = value; }
	public string PlayerName { get => Name; set => Name = value; }
	public int RoundsWon { get => RoundWin; set => RoundWin = value; }
	public int RoundsLost { get => RoundLose; set => RoundLose = value; }

	// =========================================
	// =           ROUND METHODS
	// =========================================

	public void ResetRoundData()
	{
		RoundPoints = 0;
		Killstreak = 0;
		LastKillTime = DateTime.MinValue;
	}

	public void ResetKillstreak()
	{
		Killstreak = 0;
		LastKillTime = DateTime.MinValue;
	}

	public void Reset(int startPoints)
	{
		Points = startPoints;
		Rank = 0;
		Kills = 0;
		Deaths = 0;
		Assists = 0;
		Headshots = 0;
		Shoots = 0;
		Hits = 0;
		Damage = 0;
		Playtime = 0;
		RoundsWon = 0;
		RoundsLost = 0;
		RoundsPlayed = 0;
		GameWins = 0;
		GameLosses = 0;
		GamesPlayed = 0;
		WeaponStats.Clear();
		HitData.Reset();
		ResetRoundData();
		IsDirty = false;
	}

	// =========================================
	// =           PLAYTIME TRACKING
	// =========================================

	public void UpdatePlaytime()
	{
		var sessionSeconds = (int)(DateTime.UtcNow - SessionStartTime).TotalSeconds;
		Playtime += sessionSeconds;
		SessionStartTime = DateTime.UtcNow;
		IsDirty = true;
	}

	// =========================================
	// =           GLOBAL STAT RECORDING
	// =========================================

	public void RecordGlobalHit(int damage)
	{
		Hits++;
		Damage += damage;
		IsDirty = true;
	}

	public void RecordGlobalShot()
	{
		Shoots++;
		IsDirty = true;
	}

	// =========================================
	// =           WEAPON STAT RECORDING
	// =========================================

	public void RecordWeaponHit(string weaponClassname, int damage)
	{
		var stat = WeaponStats.GetOrCreate(weaponClassname);
		stat.Hits++;
		stat.Damage += damage;
		stat.IsDirty = true;
	}

	public void RecordWeaponShot(string weaponClassname)
	{
		var stat = WeaponStats.GetOrCreate(weaponClassname);
		stat.Shots++;
		stat.IsDirty = true;
	}

	public void RecordWeaponKill(string weaponClassname, bool headshot)
	{
		var stat = WeaponStats.GetOrCreate(weaponClassname);
		stat.Kills++;

		if (headshot)
			stat.Headshots++;

		stat.IsDirty = true;
	}

	public void RecordWeaponDeath(string weaponClassname)
	{
		var stat = WeaponStats.GetOrCreate(weaponClassname);
		stat.Deaths++;
		stat.IsDirty = true;
	}
}
