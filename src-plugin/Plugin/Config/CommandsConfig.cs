namespace K4Ranks;

/// <summary>
/// Commands configuration (commands.json)
/// </summary>
public sealed class CommandsConfig
{
	/* ==================== Player Commands ==================== */

	public CommandConfig Rank { get; set; } = new()
	{
		Command = "rank",
		Aliases = ["myrank"],
		Permission = ""
	};

	public CommandConfig Ranks { get; set; } = new()
	{
		Command = "ranks",
		Aliases = ["ranklist"],
		Permission = ""
	};

	public CommandConfig Top { get; set; } = new()
	{
		Command = "top",
		Aliases = ["ranktop", "toplist"],
		Permission = ""
	};

	public CommandConfig ResetMyRank { get; set; } = new()
	{
		Command = "resetmyrank",
		Aliases = [],
		Permission = ""
	};

	public CommandConfig ToggleMessages { get; set; } = new()
	{
		Command = "togglepointmsg",
		Aliases = [],
		Permission = ""
	};

	/* ==================== Admin Commands ==================== */

	public CommandConfig SetPoints { get; set; } = new()
	{
		Command = "setpoints",
		Aliases = [],
		Permission = "k4-levelranks.admin"
	};

	public CommandConfig GivePoints { get; set; } = new()
	{
		Command = "givepoints",
		Aliases = [],
		Permission = "k4-levelranks.admin"
	};

	public CommandConfig RemovePoints { get; set; } = new()
	{
		Command = "removepoints",
		Aliases = [],
		Permission = "k4-levelranks.admin"
	};

	/* ==================== Stats Commands ==================== */

	public CommandConfig Stats { get; set; } = new()
	{
		Command = "stats",
		Aliases = ["mystats", "stat"],
		Permission = ""
	};

	public CommandConfig WeaponStats { get; set; } = new()
	{
		Command = "weaponstats",
		Aliases = [],
		Permission = ""
	};

	public CommandConfig HitStats { get; set; } = new()
	{
		Command = "hitstats",
		Aliases = [],
		Permission = ""
	};

	public CommandConfig Settings { get; set; } = new()
	{
		Command = "settings",
		Aliases = ["options"],
		Permission = ""
	};
}

/// <summary>
/// Single command configuration
/// </summary>
public sealed class CommandConfig
{
	public string Command { get; set; } = "";
	public List<string> Aliases { get; set; } = [];
	public string Permission { get; set; } = "";
}
