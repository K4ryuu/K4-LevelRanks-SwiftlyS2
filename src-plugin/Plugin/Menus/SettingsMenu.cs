using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace K4Ranks.Menus;

// =========================================
// =           SETTINGS MENU
// =========================================

/// <summary>Settings menu with toggle options</summary>
public sealed partial class MenuManager
{
	internal static class SettingsMenu
	{
		public static IMenuAPI Build(PlayerData data, ILocalizer localizer)
		{
			var menuBuilder = Plugin.Core.MenusAPI
				.CreateBuilder()
				.Design.SetMenuTitle(localizer["k4.menu.settings"])
				.Design.SetMenuTitleVisible(true)
				.Design.SetMenuFooterVisible(true)
				.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
				.SetPlayerFrozen(false);

			// =========================================
			// =           TOGGLE: POINT MESSAGES
			// =========================================

			var msgToggle = new ToggleMenuOption(localizer["k4.menu.setting.messages"], data.PointMessagesEnabled);
			msgToggle.ValueChanged += (_, e) =>
			{
				data.PointMessagesEnabled = e.NewValue;
				var msg = e.NewValue ? localizer["k4.chat.setting.messages_on"] : localizer["k4.chat.setting.messages_off"];
				e.Player.SendChat($"{localizer["k4.general.prefix"]} {msg}");
			};
			menuBuilder.AddOption(msgToggle);

			// =========================================
			// =           TOGGLE: ROUND SUMMARY
			// =========================================

			var summaryToggle = new ToggleMenuOption(localizer["k4.menu.setting.summary"], data.ShowRoundSummary);
			summaryToggle.ValueChanged += (_, e) =>
			{
				data.ShowRoundSummary = e.NewValue;
				var msg = e.NewValue ? localizer["k4.chat.setting.summary_on"] : localizer["k4.chat.setting.summary_off"];
				e.Player.SendChat($"{localizer["k4.general.prefix"]} {msg}");
			};
			menuBuilder.AddOption(summaryToggle);

			// =========================================
			// =           TOGGLE: RANK CHANGES
			// =========================================

			var rankToggle = new ToggleMenuOption(localizer["k4.menu.setting.rankchanges"], data.ShowRankChanges);
			rankToggle.ValueChanged += (_, e) =>
			{
				data.ShowRankChanges = e.NewValue;
				var msg = e.NewValue ? localizer["k4.chat.setting.rankchanges_on"] : localizer["k4.chat.setting.rankchanges_off"];
				e.Player.SendChat($"{localizer["k4.general.prefix"]} {msg}");
			};
			menuBuilder.AddOption(rankToggle);

			return menuBuilder.Build();
		}
	}
}
