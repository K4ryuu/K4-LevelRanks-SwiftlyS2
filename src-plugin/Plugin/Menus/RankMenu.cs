using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           RANK MENU
// =========================================

/// <summary>Rank details - shows rank info</summary>
public sealed partial class MenuManager
{
	internal static class RankMenu
	{
		// =========================================
		// =           BUILD MENU (sync, for SubmenuMenuOption)
		// =========================================

		public static IMenuAPI Build(MenuManager manager, IPlayer player, PlayerData data, ILocalizer localizer)
		{
			var rank = manager._ranks.GetRank(data.Points);
			var nextRank = manager._ranks.GetNextRank(data.Points);

			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.myrank"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.current_rank"], $"<font color='{rank.Hex}'>{rank.Name}</font>"]
			));

			menuBuilder.AddOption(new TextMenuOption(
				localizer["k4.format.line", localizer["k4.label.points"], data.Points]
			));

			if (nextRank != null)
			{
				var pointsNeeded = nextRank.Points - data.Points;
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.next_rank"], $"<font color='{nextRank.Hex}'>{nextRank.Name}</font>"]
				));
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.points_needed"], pointsNeeded]
				));
			}
			else
			{
				menuBuilder.AddOption(new TextMenuOption(
					localizer["k4.format.line", localizer["k4.label.max_rank"], localizer["k4.format.max_rank"]]
				));
			}

			return menuBuilder.Build();
		}
	}
}
