using K4Ranks.Menus;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SteamAPI;

namespace K4Ranks;

public sealed partial class Plugin
{
	/* ==================== State ==================== */

	private MenuManager? _menuManager;

	/* ==================== Command Registration ==================== */

	private void RegisterCommands()
	{
		_menuManager = new MenuManager(Database, Modules.CurrentValue, Ranks);

		// Player commands
		RegisterCommandWithAliases(Commands.CurrentValue.Rank, OnRankCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.Ranks, OnRanksCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.Top, OnTopCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.ResetMyRank, OnResetMyRankCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.ToggleMessages, OnToggleMessagesCommand);

		// Admin commands
		RegisterCommandWithAliases(Commands.CurrentValue.SetPoints, OnSetPointsCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.GivePoints, OnGivePointsCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.RemovePoints, OnRemovePointsCommand);

		// Stat submenu commands
		RegisterCommandWithAliases(Commands.CurrentValue.Stats, OnStatsCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.WeaponStats, OnWeaponStatsCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.HitStats, OnHitStatsCommand);
		RegisterCommandWithAliases(Commands.CurrentValue.Settings, OnSettingsCommand);
	}

	private static void RegisterCommandWithAliases(CommandConfig config, ICommandService.CommandListener handler)
	{
		if (string.IsNullOrWhiteSpace(config.Command))
			return;

		Core.Command.RegisterCommand(config.Command, handler);

		foreach (var alias in config.Aliases)
		{
			if (!string.IsNullOrWhiteSpace(alias))
				Core.Command.RegisterCommandAlias(config.Command, alias);
		}
	}

	/* ==================== Player Commands ==================== */

	private void OnRankCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		_menuManager?.OpenMainMenu(player, data!);
	}

	private void OnRanksCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		if (_menuManager != null)
		{
			var menu = MenuManager.RanksListMenu.Build(_menuManager, player, data!, localizer);
			Core.MenusAPI.OpenMenuForPlayer(player, menu);
		}
	}

	private void OnTopCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		if (_menuManager != null)
		{
			var menu = MenuManager.TopPlayersMenu.Build(_menuManager, player, localizer);
			Core.MenusAPI.OpenMenuForPlayer(player, menu);
		}
	}

	private void OnResetMyRankCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		Task.Run(async () => await ResetPlayerRankAsync(player, data!));
	}

	private async Task ResetPlayerRankAsync(IPlayer player, PlayerData data)
	{
		var visibleSteamId = SteamIdParser.ToSteamId(player.SteamID);
		await Database.ResetPlayerAsync(visibleSteamId);

		data.Reset(Config.CurrentValue.Rank.StartPoints);

		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			UpdatePlayerClanTag(player, data);

			var localizer = Core.Translation.GetPlayerLocalizer(player);
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.rank.reset"]}");
		});
	}

	private void OnToggleMessagesCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		data!.PointMessagesEnabled = !data.PointMessagesEnabled;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		var messageKey = data.PointMessagesEnabled
			? "k4.chat.setting.messages_on"
			: "k4.chat.setting.messages_off";

		player.SendChat($"{localizer["k4.general.prefix"]} {localizer[messageKey]}");
	}

	/* ==================== Admin Commands ==================== */

	private void OnSetPointsCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);

		if (ctx.Args.Length < 2)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.usage.setpoints"]}");
			return;
		}

		if (!int.TryParse(ctx.Args[1], out var points))
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.error.invalid_points"]}");
			return;
		}

		var target = FindSingleTarget(player, ctx.Args[0], localizer);
		if (target == null)
			return;

		var data = PlayerData.GetPlayerData(target);
		if (data == null || !data.IsLoaded)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.error.target_not_loaded"]}");
			return;
		}

		data.Points = points;
		data.IsDirty = true;

		UpdatePlayerClanTag(target, data);

		var targetName = target.Controller?.PlayerName ?? "Unknown";
		player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.admin.points_set", targetName, points]}");

		var targetLocalizer = Core.Translation.GetPlayerLocalizer(target);
		target.SendChat($"{targetLocalizer["k4.general.prefix"]} {targetLocalizer["k4.admin.points_set_target", points]}");
	}

	private void OnGivePointsCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);

		if (ctx.Args.Length < 2)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.usage.givepoints"]}");
			return;
		}

		if (!int.TryParse(ctx.Args[1], out var points) || points <= 0)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.error.invalid_points"]}");
			return;
		}

		var target = FindSingleTarget(player, ctx.Args[0], localizer);
		if (target == null)
			return;

		var targetLocalizer = Core.Translation.GetPlayerLocalizer(target);
		PlayerData.ModifyPoints(target, points, targetLocalizer["k4.reason.admin"]);

		var targetName = target.Controller?.PlayerName ?? "Unknown";
		player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.admin.points_given", points, targetName]}");
	}

	private void OnRemovePointsCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);

		if (ctx.Args.Length < 2)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.usage.removepoints"]}");
			return;
		}

		if (!int.TryParse(ctx.Args[1], out var points) || points <= 0)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.error.invalid_points"]}");
			return;
		}

		var target = FindSingleTarget(player, ctx.Args[0], localizer);
		if (target == null)
			return;

		var targetLocalizer = Core.Translation.GetPlayerLocalizer(target);
		PlayerData.ModifyPoints(target, -points, targetLocalizer["k4.reason.admin"]);

		var targetName = target.Controller?.PlayerName ?? "Unknown";
		player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.admin.points_removed", points, targetName]}");
	}

	/* ==================== Stats Commands ==================== */

	private void OnStatsCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		if (_menuManager != null)
		{
			var menu = MenuManager.StatsCategoryMenu.Build(_menuManager, player, data!, localizer);
			Core.MenusAPI.OpenMenuForPlayer(player, menu);
		}
	}

	private void OnWeaponStatsCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		if (!Modules.CurrentValue.WeaponStatsEnabled)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		if (_menuManager != null)
		{
			var menu = MenuManager.WeaponStatsMenu.Build(player, data!, localizer);
			Core.MenusAPI.OpenMenuForPlayer(player, menu);
		}
	}

	private void OnHitStatsCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		if (!Modules.CurrentValue.HitStatsEnabled)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		var menu = MenuManager.HitStatsMenu.Build(player, data!, localizer);
		Core.MenusAPI.OpenMenuForPlayer(player, menu);
	}

	private void OnSettingsCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;
		if (player == null || !player.IsValid)
			return;

		var data = PlayerData.GetPlayerData(player);
		if (!ValidatePlayerData(player, data))
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		var menu = MenuManager.SettingsMenu.Build(data!, localizer);
		Core.MenusAPI.OpenMenuForPlayer(player, menu);
	}

	/* ==================== Helpers ==================== */

	private bool ValidatePlayerData(IPlayer player, PlayerData? data)
	{
		if (data != null && data.IsLoaded)
			return true;

		var localizer = Core.Translation.GetPlayerLocalizer(player);
		player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.error.data_not_loaded"]}");
		return false;
	}

	private IPlayer? FindSingleTarget(IPlayer sender, string targetArg, SwiftlyS2.Shared.Translation.ILocalizer localizer)
	{
		var target = Core.PlayerManager
			.FindTargettedPlayers(sender, targetArg, TargetSearchMode.NoMultipleTargets)
			.FirstOrDefault();

		if (target == null)
			sender.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.error.no_target"]}");

		return target;
	}

	private void UpdatePlayerClanTag(IPlayer player, PlayerData data)
	{
		if (!Config.CurrentValue.Scoreboard.Clantags)
			return;

		var rank = Ranks.GetRank(data.Points);
		var controller = player.Controller;

		if (controller != null)
		{
			controller.Clan = rank.Tag;
			controller.ClanUpdated();
		}
	}
}
