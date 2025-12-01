using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           RANKS CATEGORY MENU
// =========================================

/// <summary>Ranks category menu - My Rank, Top Players, All Ranks</summary>
public sealed partial class MenuManager
{
	internal static class RanksCategoryMenu
	{
		public static IMenuAPI Build(MenuManager manager, IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.category.ranks"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			// My Rank
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.myrank"],
				() => RankMenu.Build(manager, player, data, localizer)
			));

			// Top Players
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.top"],
				() => TopPlayersMenu.Build(manager, player, localizer)
			));

			// All Ranks
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.allranks"],
				() => RanksListMenu.Build(manager, player, data, localizer)
			));

			return menuBuilder.Build();
		}
	}
}
