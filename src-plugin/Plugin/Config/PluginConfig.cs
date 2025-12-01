namespace K4Ranks;

/// <summary>
/// Main plugin configuration (config.json)
/// </summary>
public sealed class PluginConfig
{
	public DatabaseSettings Database { get; set; } = new();
	public RankSettings Rank { get; set; } = new();
	public ScoreboardSettings Scoreboard { get; set; } = new();
	public PointSystemSettings Points { get; set; } = new();
	public VipSettings Vip { get; set; } = new();
}

/* ==================== Database ==================== */

public sealed class DatabaseSettings
{
	/// <summary>DB connection name (from SwiftlyS2's database.jsonc)</summary>
	public string Connection { get; set; } = "host";

	/// <summary>Days to keep inactive player records (0 = forever)</summary>
	public int PurgeDays { get; set; } = 30;
}

/* ==================== Rank ==================== */

public sealed class RankSettings
{
	/// <summary>Starting points for new players</summary>
	public int StartPoints { get; set; } = 0;

	/// <summary>Minimum players required for points to be awarded</summary>
	public int MinPlayers { get; set; } = 4;

	/// <summary>Allow points during warmup</summary>
	public bool WarmupPoints { get; set; } = false;

	/// <summary>Award points for killing bots</summary>
	public bool PointsForBots { get; set; } = false;

	/// <summary>FFA mode (no team-based penalties)</summary>
	public bool FFAMode { get; set; } = false;
}

/* ==================== Scoreboard ==================== */

public sealed class ScoreboardSettings
{
	/// <summary>Show rank in scoreboard clan tags</summary>
	public bool Clantags { get; set; } = true;

	/// <summary>Sync player score with points</summary>
	public bool ScoreSync { get; set; } = false;

	/// <summary>Show competitive ranks on scoreboard (requires FollowCS2ServerGuidelines = false)</summary>
	public bool UseRanks { get; set; } = true;

	/// <summary>Scoreboard rank mode: 1 = Premier (points), 2 = Competitive, 3 = Wingman, 4 = Danger Zone, 0 = Custom</summary>
	public int RankMode { get; set; } = 1;

	/// <summary>Base rank value for custom mode (mode 0)</summary>
	public int CustomRankBase { get; set; } = 0;

	/// <summary>Maximum rank value for custom mode (mode 0)</summary>
	public int CustomRankMax { get; set; } = 18;

	/// <summary>Rank margin for custom mode (mode 0)</summary>
	public int CustomRankMargin { get; set; } = 0;

	/// <summary>Interval in seconds for reveal all message (2-5 recommended)</summary>
	public float RevealAllInterval { get; set; } = 3.0f;
}

/* ==================== Point System ==================== */

public sealed class PointSystemSettings
{
	/// <summary>Show round-end point summary instead of per-action messages</summary>
	public bool RoundEndSummary { get; set; } = false;

	/// <summary>Reset killstreak on round end</summary>
	public bool KillstreakResetOnRoundEnd { get; set; } = false;

	/// <summary>Show killer/victim names in point messages</summary>
	public bool ShowPlayerNames { get; set; } = false;

	/// <summary>Dynamic points based on victim/attacker point ratio</summary>
	public bool DynamicDeathPoints { get; set; } = true;

	/// <summary>Minimum multiplier for dynamic death points</summary>
	public double DynamicDeathPointsMin { get; set; } = 0.5;

	/// <summary>Maximum multiplier for dynamic death points</summary>
	public double DynamicDeathPointsMax { get; set; } = 3.0;
}

/* ==================== VIP ==================== */

public sealed class VipSettings
{
	/// <summary>VIP point multiplier</summary>
	public double Multiplier { get; set; } = 1.25;

	/// <summary>Flags that grant VIP status (any one of these)</summary>
	public List<string> Flags { get; set; } = ["k4-levelranks.vip"];
}
