using Microsoft.Extensions.Logging;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           WEAPON STATS MENU
// =========================================

/// <summary>Weapon stats menu - list weapons, show details in submenu</summary>
public sealed partial class MenuManager
{
	internal static class WeaponStatsMenu
	{
		// =========================================
		// =           BUILD MENU
		// =========================================

		public static IMenuAPI Build(IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.weaponstats"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			var weapons = data.WeaponStats.GetAll()
				.OrderByDescending(w => w.Kills)
				.Take(15)
				.ToList();

			if (weapons.Count == 0)
			{
				menuBuilder.AddOption(new TextMenuOption(localizer["k4.menu.weapon.nodata"]));
			}
			else
			{
				foreach (var weapon in weapons)
				{
					var displayName = GetWeaponDisplayName(weapon.WeaponClassname, localizer);
					var w = weapon;

					menuBuilder.AddOption(new SubmenuMenuOption(
						displayName,
						() => BuildWeaponDetailsMenu(w, localizer)
					));
				}
			}

			return menuBuilder.Build();
		}

		// =========================================
		// =           BUILD DETAILS MENU
		// =========================================

		private static IMenuAPI BuildWeaponDetailsMenu(WeaponStat weapon, ILocalizer localizer)
		{
			var displayName = GetWeaponDisplayName(weapon.WeaponClassname, localizer);
			var accuracy = weapon.Shots > 0 ? Math.Round((double)weapon.Hits / weapon.Shots * 100, 1) : 0;
			var hsPercent = weapon.Kills > 0 ? Math.Round((double)weapon.Headshots / weapon.Kills * 100, 1) : 0;

			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(displayName)
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.kills"], weapon.Kills]
			));
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.deaths"], weapon.Deaths]
			));

			// Headshots with percent in parentheses
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.headshots"], localizer["k4.format.value_percent", weapon.Headshots, hsPercent]]
			));

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.shots"], weapon.Shots]
			));

			// Hits with accuracy percent in parentheses
			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.hits"], localizer["k4.format.value_percent", weapon.Hits, accuracy]]
			));

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.damage"], weapon.Damage]
			));

			return menuBuilder.Build();
		}

		// =========================================
		// =           HELPER
		// =========================================

		private static string GetWeaponDisplayName(string weaponClassname, ILocalizer localizer)
		{
			var key = weaponClassname.Replace("weapon_", "");
			var locKey = $"k4.weapon.{key}";

			try
			{
				var translated = localizer[locKey];

				// Some localizers return the key if not found
				if (translated == locKey || translated.Contains("k4.weapon."))
				{
					Plugin.Core.Logger.LogWarning($"Missing weapon translation: '{locKey}' - Please report this to the developer!");
					return key;
				}

				return translated;
			}
			catch
			{
				Plugin.Core.Logger.LogWarning($"Missing weapon translation: '{locKey}' - Please report this to the developer!");
				return key;
			}
		}
	}
}
