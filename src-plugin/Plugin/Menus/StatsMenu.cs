using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           STATS MENU
// =========================================

/// <summary>Stats submenu - combat, rounds, games, accuracy</summary>
public sealed partial class MenuManager
{
	internal static class StatsMenu
	{
		// =========================================
		// =           BUILD MENU
		// =========================================

		public static IMenuAPI Build(MenuManager manager, IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.mystats"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			// Combat Stats
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.stats.combat"],
				() => BuildCombatStats(player, data, localizer)
			));

			// Round Stats
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.stats.rounds"],
				() => BuildRoundStats(player, data, localizer)
			));

			// Game Stats
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.stats.games"],
				() => BuildGameStats(player, data, localizer)
			));

			// Accuracy Stats
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.stats.accuracy"],
				() => BuildAccuracyStats(player, data, localizer)
			));

			return menuBuilder.Build();
		}

		// =========================================
		// =           BUILD COMBAT STATS
		// =========================================

		internal static IMenuAPI BuildCombatStats(IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.stats.combat"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.kills"], data.Kills]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.deaths"], data.Deaths]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.kdr"], data.KDR]
			));

			// Headshots: show as count (percent)
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.headshots"], localizer["k4.format.value_percent", data.Headshots, data.HeadshotPercentage]]
			));

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.assists"], data.Assists]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.damage"], data.Damage]
			));

			return menuBuilder.Build();
		}

		// =========================================
		// =           BUILD ROUND STATS
		// =========================================

		internal static IMenuAPI BuildRoundStats(IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.stats.rounds"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			var winRate = data.RoundsPlayed > 0
				? Math.Round((double)data.RoundsWon / data.RoundsPlayed * 100, 1)
				: 0;

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.rounds_played"], data.RoundsPlayed]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.rounds_won"], data.RoundsWon]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.rounds_lost"], data.RoundsLost]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.winrate"], localizer["k4.format.percent", winRate]]
			));

			return menuBuilder.Build();
		}

		// =========================================
		// =           BUILD GAME STATS
		// =========================================

		internal static IMenuAPI BuildGameStats(IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.stats.games"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			var winRate = data.GamesPlayed > 0
				? Math.Round((double)data.GameWins / data.GamesPlayed * 100, 1)
				: 0;

			var playtimeHours = Math.Round(data.Playtime / 3600.0, 1);

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.games_played"], data.GamesPlayed]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.games_won"], data.GameWins]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.games_lost"], data.GameLosses]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.winrate"], localizer["k4.format.percent", winRate]]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.playtime"], localizer["k4.format.hours", playtimeHours]]
			));

			return menuBuilder.Build();
		}

		// =========================================
		// =           BUILD ACCURACY STATS
		// =========================================

		internal static IMenuAPI BuildAccuracyStats(IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.stats.accuracy"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.shots"], data.Shoots]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.hits"], data.Hits]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.accuracy"], localizer["k4.format.percent", data.Accuracy]]
			));

			return menuBuilder.Build();
		}
	}
}
