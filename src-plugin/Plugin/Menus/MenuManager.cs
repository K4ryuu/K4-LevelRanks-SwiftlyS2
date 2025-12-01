using SwiftlyS2.Shared.Players;

namespace K4Ranks.Menus;

/// <summary>
/// Central menu manager for K4-LevelRanks
/// Handles all menu navigation and display
/// Split into partial classes for organization
/// </summary>
public sealed partial class MenuManager
{
	// =========================================
	// =           FIELDS
	// =========================================

	internal readonly Plugin.DatabaseService _database;
	internal readonly ModuleConfig _modules;
	internal readonly Plugin.RankService _ranks;

	// =========================================
	// =           CONSTRUCTOR
	// =========================================

	public MenuManager(Plugin.DatabaseService database, ModuleConfig modules, Plugin.RankService ranks)
	{
		_database = database;
		_modules = modules;
		_ranks = ranks;
	}

	// =========================================
	// =           PUBLIC API
	// =========================================

	/// <summary>Opens the main stats menu for a player</summary>
	public void OpenMainMenu(IPlayer player, PlayerData data)
	{
		MainMenu.Show(this, player, data);
	}
}
