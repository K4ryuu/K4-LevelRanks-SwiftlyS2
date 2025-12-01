using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           LOADING MENU HELPER
// =========================================

/// <summary>Helper for async menus - shows loading state then opens real menu</summary>
public sealed partial class MenuManager
{
	internal static class LoadingMenu
	{
		// =========================================
		// =           BUILD WITH ASYNC LOAD
		// =========================================

		/// <summary>
		/// Creates a loading menu that displays while async data loads,
		/// then automatically opens the real menu when ready.
		/// </summary>
		/// <typeparam name="T">Type of data being loaded</typeparam>
		/// <param name="title">Menu title</param>
		/// <param name="localizer">Localizer for translations</param>
		/// <param name="player">Target player</param>
		/// <param name="loadDataAsync">Async function to load data</param>
		/// <param name="buildRealMenu">Function to build the real menu with loaded data</param>
		/// <returns>Loading menu that will be replaced with real menu</returns>
		public static IMenuAPI Build<T>(
			string title,
			ILocalizer localizer,
			IPlayer player,
			Func<Task<T>> loadDataAsync,
			Func<T, IMenuAPI> buildRealMenu)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(title)
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			menuBuilder.AddOption(new TextMenuOption(localizer["k4.menu.loading"]));

			var loadingMenu = menuBuilder.Build();

			Task.Run(async () =>
			{
				var data = await loadDataAsync();

				Plugin.Core.Scheduler.NextWorldUpdate(() =>
				{
					if (!player.IsValid)
						return;

					Plugin.Core.MenusAPI.CloseMenuForPlayer(player, loadingMenu);
					var realMenu = buildRealMenu(data);
					Plugin.Core.MenusAPI.OpenMenuForPlayer(player, realMenu);
				});
			});

			return loadingMenu;
		}
	}
}
