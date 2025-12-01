using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           RANKS LIST MENU
// =========================================

/// <summary>All ranks list menu</summary>
public sealed partial class MenuManager
{
	internal static class RanksListMenu
	{
		public static IMenuAPI Build(MenuManager manager, IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.allranks"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			foreach (var rank in manager._ranks.Ranks)
			{
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", rank.Name, localizer["k4.format.points", rank.Points]]
				));
			}

			return menuBuilder.Build();
		}
	}
}
