namespace K4Ranks;

/// <summary>
/// Utility class for formatting playtime into human-readable format
/// </summary>
internal static class PlaytimeFormatter
{
	/// <summary>
	/// Formats playtime seconds into readable format (days, hours, minutes)
	/// </summary>
	/// <param name="playtimeSeconds">Total playtime in seconds</param>
	/// <returns>Formatted string like "4d 5h 30m" or "5h 30m" or "30m"</returns>
	public static string Format(long playtimeSeconds)
	{
		var days = playtimeSeconds / 86400;
		var hours = (playtimeSeconds % 86400) / 3600;
		var minutes = (playtimeSeconds % 3600) / 60;

		if (days > 0)
			return $"{days}d {hours}h {minutes}m";
		else if (hours > 0)
			return $"{hours}h {minutes}m";
		else
			return $"{minutes}m";
	}
}
