using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace K4Ranks.Menus;

// =========================================
// =           MAIN MENU
// =========================================

/// <summary>Main menu - entry point with categories</summary>
public sealed partial class MenuManager
{
	internal static class MainMenu
	{
		public static void Show(MenuManager manager, IPlayer player, PlayerData data)
		{
			var localizer = Plugin.Core.Translation.GetPlayerLocalizer(player);

			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.title"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			// Ranks Category
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.category.ranks"],
				() => RanksCategoryMenu.Build(manager, player, data, localizer)
			));

			// Statistics Category
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.category.stats"],
				() => StatsCategoryMenu.Build(manager, player, data, localizer)
			));

			// Settings Category
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.settings"],
				() => SettingsMenu.Build(data, localizer)
			));

			var menu = menuBuilder.Build();
			Plugin.Core.MenusAPI.OpenMenuForPlayer(player, menu);
		}
	}
}
