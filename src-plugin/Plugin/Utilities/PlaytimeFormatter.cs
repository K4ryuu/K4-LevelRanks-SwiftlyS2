using SwiftlyS2.Shared.Translation;

namespace K4Ranks;

/// <summary>
/// Utility class for formatting playtime into human-readable format
/// </summary>
internal static class PlaytimeFormatter
{
	/// <summary>
	/// Formats playtime seconds into readable format (days, hours, minutes) with localization
	/// </summary>
	/// <param name="playtimeSeconds">Total playtime in seconds</param>
	/// <param name="localizer">Localizer for translation</param>
	/// <returns>Formatted string like "4d 5h 30m" or "5h 30m" or "30m"</returns>
	public static string Format(long playtimeSeconds, ILocalizer localizer)
	{
		var days = playtimeSeconds / 86400;
		var hours = playtimeSeconds % 86400 / 3600;
		var minutes = playtimeSeconds % 3600 / 60;

		var dayStr = localizer["k4.format.day"];
		var hourStr = localizer["k4.format.hour"];
		var minStr = localizer["k4.format.minute"];

		if (days > 0)
			return $"{days}{dayStr} {hours}{hourStr} {minutes}{minStr}";
		else if (hours > 0)
			return $"{hours}{hourStr} {minutes}{minStr}";
		else
			return $"{minutes}{minStr}";
	}
}
