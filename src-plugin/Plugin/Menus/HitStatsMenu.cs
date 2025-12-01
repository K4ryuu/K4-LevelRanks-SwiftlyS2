using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           HIT STATS MENU
// =========================================

/// <summary>Hit stats - shows hitbox distribution</summary>
public sealed partial class MenuManager
{
	internal static class HitStatsMenu
	{
		// =========================================
		// =           BUILD MENU
		// =========================================

		internal static IMenuAPI Build(IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.hitstats"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			var hitData = data.HitData;
			var total = hitData.TotalHits;

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.total"], total]
			));

			if (total > 0)
			{
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.head"], localizer["k4.format.value_percent", hitData.Head, GetHitPercent(hitData.Head, total)]]
				));
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.chest"], localizer["k4.format.value_percent", hitData.Chest, GetHitPercent(hitData.Chest, total)]]
				));
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.stomach"], localizer["k4.format.value_percent", hitData.Belly, GetHitPercent(hitData.Belly, total)]]
				));
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.arms"], localizer["k4.format.value_percent", hitData.LeftArm + hitData.RightArm, GetHitPercent(hitData.LeftArm + hitData.RightArm, total)]]
				));
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.legs"], localizer["k4.format.value_percent", hitData.LeftLeg + hitData.RightLeg, GetHitPercent(hitData.LeftLeg + hitData.RightLeg, total)]]
				));
			}

			return menuBuilder.Build();
		}

		// =========================================
		// =           HELPER
		// =========================================

		private static double GetHitPercent(int hits, int total) =>
			total > 0 ? Math.Round((double)hits / total * 100, 1) : 0;
	}
}
