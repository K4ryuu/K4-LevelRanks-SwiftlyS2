using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           STATS CATEGORY MENU
// =========================================

/// <summary>Stats category menu - My Stats, Weapon Stats, Hit Stats</summary>
public sealed partial class MenuManager
{
	internal static class StatsCategoryMenu
	{
		public static IMenuAPI Build(MenuManager manager, IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.category.stats"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			// My Stats (submenu)
			menuBuilder.AddOption(new SubmenuMenuOption(
				localizer["k4.menu.mystats"],
				() => StatsMenu.Build(manager, player, data, localizer)
			));

			// Weapon Stats (submenu, if enabled)
			if (manager._modules.WeaponStatsEnabled)
			{
				menuBuilder.AddOption(new SubmenuMenuOption(
					localizer["k4.menu.weaponstats"],
					() => WeaponStatsMenu.Build(player, data, localizer)
				));
			}

			// Hit Stats (opens menu, if enabled)
			if (manager._modules.HitStatsEnabled)
			{
				menuBuilder.AddOption(new SubmenuMenuOption(
					localizer["k4.menu.hitstats"],
					() => HitStatsMenu.Build(player, data, localizer)
				));
			}

			return menuBuilder.Build();
		}
	}
}
