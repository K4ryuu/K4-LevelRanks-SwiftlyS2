using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           TOP PLAYERS MENU
// =========================================

/// <summary>Top players leaderboard menu</summary>
public sealed partial class MenuManager
{
	internal static class TopPlayersMenu
	{
		// =========================================
		// =           BUILD MENU (for SubmenuMenuOption)
		// =========================================

		public static IMenuAPI Build(MenuManager manager, IPlayer player, ILocalizer localizer)
		{
			return LoadingMenu.Build(
				localizer["k4.menu.top"],
				localizer,
				player,
				() => manager._database.GetTopPlayersAsync(10),
				topPlayers => BuildDetailsMenu(manager, localizer, topPlayers)
			);
		}

		// =========================================
		// =           BUILD DETAILS MENU
		// =========================================

		private static IMenuAPI BuildDetailsMenu(MenuManager manager, ILocalizer localizer, List<PlayerData> topPlayers)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.top"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			if (topPlayers.Count == 0)
			{
				menuBuilder.AddOption(new TextMenuOption(localizer["k4.menu.top.empty"]));
			}
			else
			{
				for (int i = 0; i < topPlayers.Count; i++)
				{
					var p = topPlayers[i];
					var rank = manager._ranks.GetRank(p.Points);
					var position = i + 1;

					// Format: #1 <rank colored name> - <gold points> pts
					var formattedEntry = $"#{position} <font color='{rank.Hex}'>{p.PlayerName}</font> - <font color='#FFD700'>{p.Points}</font> pts";

					menuBuilder.AddOption(new TextMenuOption(formattedEntry));
				}
			}

			return menuBuilder.Build();
		}
	}
}
