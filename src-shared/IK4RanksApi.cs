using SwiftlyS2.Shared.Players;

namespace K4RanksSharedApi;

// =========================================
// =           K4-LEVELRANKS API
// =========================================

/// <summary>
/// Public API interface for K4-LevelRanks plugin.
/// Allows external plugins to interact with player stats, points, and rankings.
/// </summary>
public interface IK4RanksApi
{
	// =========================================
	// =           PLAYER STATS
	// =========================================

	/// <summary>Get any player stat by field name.</summary>
	/// <param name="player">The player to get the stat for.</param>
	/// <param name="field">The field name (e.g., "Kills", "Deaths", "Points").</param>
	/// <returns>The stat value, or null if not found.</returns>
	object? GetPlayerStat(IPlayer player, string field);

	/// <summary>Check if player data is loaded.</summary>
	/// <param name="player">The player to check.</param>
	/// <returns>True if player data is loaded, false otherwise.</returns>
	bool IsPlayerDataLoaded(IPlayer player);

	// =========================================
	// =           POINTS MANAGEMENT
	// =========================================

	/// <summary>Modify player's points by an amount.</summary>
	/// <param name="player">The player to modify points for.</param>
	/// <param name="amount">The amount to add (positive) or subtract (negative).</param>
	/// <param name="reason">The reason for the modification.</param>
	/// <param name="showMessage">Whether to show a chat message to the player.</param>
	void ModifyPlayerPoints(IPlayer player, int amount, string reason, bool showMessage = true);

	/// <summary>Set player's points to an exact value.</summary>
	/// <param name="player">The player to set points for.</param>
	/// <param name="points">The exact point value to set.</param>
	void SetPlayerPoints(IPlayer player, int points);

	// =========================================
	// =           RANKINGS (ASYNC)
	// =========================================

	/// <summary>Get player's rank position.</summary>
	/// <param name="player">The player to get the position for.</param>
	/// <returns>The player's rank position (1-based), or 0 if not ranked.</returns>
	Task<int> GetPlayerPositionAsync(IPlayer player);

	/// <summary>Get total number of ranked players.</summary>
	/// <returns>The total number of players in the ranking system.</returns>
	Task<int> GetTotalPlayersAsync();
}
