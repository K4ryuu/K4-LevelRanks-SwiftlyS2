using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

/// <summary>Top players by time (playtime) menu</summary>
public sealed partial class MenuManager
{
	internal static class TopPlayersTimeMenu
	{
		public static IMenuAPI Build(MenuManager manager, IPlayer player, ILocalizer localizer)
		{
			return LoadingMenu.Build(
				localizer["k4.menu.ttop"],
				localizer,
				player,
				() => manager._database.GetTopPlayersByTimeAsync(10),
				topPlayers => BuildDetailsMenu(manager, localizer, topPlayers)
			);
		}

		private static IMenuAPI BuildDetailsMenu(MenuManager manager, ILocalizer localizer, List<PlayerData> topPlayers)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.ttop"])
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
					var position = i + 1;
					var formattedTime = PlaytimeFormatter.Format(p.Playtime, localizer);

					// Format: #1 <name> - <gold>XdXhXm</gold>
					var formattedEntry = $"#{position} {p.PlayerName} - <font color='#FFD700'>{formattedTime}</font>";

					menuBuilder.AddOption(new TextMenuOption(formattedEntry));
				}
			}

			return menuBuilder.Build();
		}
	}
}
