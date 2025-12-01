namespace K4Ranks;

/// <summary>
/// Ranks configuration (ranks.json)
/// </summary>
public sealed class RanksConfig
{
	public List<Rank> Ranks { get; set; } =
	[
		// Silver Ranks
		new() { Name = "Silver I", Tag = "S1", Color = "[gray]", Hex = "#808080", Points = 0 },
		new() { Name = "Silver II", Tag = "S2", Color = "[gray]", Hex = "#808080", Points = 100 },
		new() { Name = "Silver III", Tag = "S3", Color = "[gray]", Hex = "#808080", Points = 200 },
		new() { Name = "Silver IV", Tag = "S4", Color = "[gray]", Hex = "#808080", Points = 350 },
		new() { Name = "Silver Elite", Tag = "SE", Color = "[gray]", Hex = "#808080", Points = 500 },
		new() { Name = "Silver Elite Master", Tag = "SEM", Color = "[gray]", Hex = "#808080", Points = 750 },

		// Gold Nova Ranks
		new() { Name = "Gold Nova I", Tag = "GN1", Color = "[gold]", Hex = "#FFD700", Points = 1000 },
		new() { Name = "Gold Nova II", Tag = "GN2", Color = "[gold]", Hex = "#FFD700", Points = 1250 },
		new() { Name = "Gold Nova III", Tag = "GN3", Color = "[gold]", Hex = "#FFD700", Points = 1500 },
		new() { Name = "Gold Nova Master", Tag = "GNM", Color = "[gold]", Hex = "#FFD700", Points = 1750 },

		// Master Guardian Ranks
		new() { Name = "Master Guardian I", Tag = "MG1", Color = "[lightblue]", Hex = "#87CEEB", Points = 2000 },
		new() { Name = "Master Guardian II", Tag = "MG2", Color = "[lightblue]", Hex = "#87CEEB", Points = 2500 },
		new() { Name = "Master Guardian Elite", Tag = "MGE", Color = "[lightblue]", Hex = "#87CEEB", Points = 3000 },
		new() { Name = "Distinguished Master Guardian", Tag = "DMG", Color = "[blue]", Hex = "#0000FF", Points = 3500 },

		// Elite Ranks
		new() { Name = "Legendary Eagle", Tag = "LE", Color = "[purple]", Hex = "#800080", Points = 4000 },
		new() { Name = "Legendary Eagle Master", Tag = "LEM", Color = "[purple]", Hex = "#800080", Points = 5000 },
		new() { Name = "Supreme Master First Class", Tag = "SMFC", Color = "[lightred]", Hex = "#FF6B6B", Points = 6000 },
		new() { Name = "Global Elite", Tag = "GE", Color = "[red]", Hex = "#FF0000", Points = 7500 }
	];
}

/// <summary>
/// Single rank definition
/// </summary>
public sealed class Rank
{
	/// <summary>Display name of the rank</summary>
	public string Name { get; set; } = "";

	/// <summary>Short tag for scoreboard clan tag</summary>
	public string Tag { get; set; } = "";

	/// <summary>Chat color code (e.g., [red], [lime], [gold])</summary>
	public string Color { get; set; } = "[white]";

	/// <summary>Hex color for menu display (e.g., #FF0000)</summary>
	public string Hex { get; set; } = "#FFFFFF";

	/// <summary>Minimum points required for this rank</summary>
	public int Points { get; set; } = 0;

	/// <summary>Chat color (alias for Color)</summary>
	public string ChatColor => Color;
}
