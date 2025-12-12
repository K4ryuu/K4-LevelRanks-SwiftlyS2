using K4RanksSharedApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;

namespace K4Ranks;

[PluginMetadata(
	Id = "k4.levelranks",
	Version = "1.0.3",
	Name = "K4 - Level Ranks",
	Author = "K4ryuu",
	Description = "Experience-based ranking system with configurable ranks, detailed player statistics, weapon tracking, and hit analysis for CS2."
)]
public sealed partial class Plugin(ISwiftlyCore core) : BasePlugin(core)
{
	/* ==================== Static Access ==================== */

	public static new ISwiftlyCore Core { get; private set; } = null!;

	/* ==================== Configurations ==================== */

	internal IOptionsMonitor<PluginConfig> Config { get; private set; } = null!;
	internal IOptionsMonitor<PointsConfig> Points { get; private set; } = null!;
	internal IOptionsMonitor<RanksConfig> RanksConfig { get; private set; } = null!;
	internal IOptionsMonitor<CommandsConfig> Commands { get; private set; } = null!;
	internal IOptionsMonitor<ModuleConfig> Modules { get; private set; } = null!;

	/* ==================== Services ==================== */

	internal RankService Ranks { get; private set; } = null!;
	internal DatabaseService Database { get; private set; } = null!;
	internal PlayerDataService PlayerData { get; private set; } = null!;
	internal ScoreboardService Scoreboard { get; private set; } = null!;

	/* ==================== Game Rules ==================== */

	internal bool IsWarmup => Core.EntitySystem.GetGameRules()?.WarmupPeriod == true;

	/* ==================== Plugin Lifecycle ==================== */

	public override void Load(bool hotReload)
	{
		Core = base.Core;

		InitializeConfigs();
		InitializeServices();
		InitializeDatabase();

		RegisterEvents();
		RegisterCommands();

		Scoreboard.Start();

		if (hotReload)
			HandleHotReload();
	}

	private void InitializeServices()
	{
		Database = new DatabaseService(
			Config.CurrentValue.Database.Connection,
			Config.CurrentValue.Database.PurgeDays,
			Config.CurrentValue.Rank.StartPoints,
			Modules.CurrentValue
		);

		Ranks = new RankService(RanksConfig.CurrentValue);
		PlayerData = new PlayerDataService(this);
		Scoreboard = new ScoreboardService(this);
	}

	private void InitializeDatabase()
	{
		Task.Run(async () =>
		{
			await Database.InitializeAsync();
			await Database.PurgeOldDataAsync();
		});
	}

	private void HandleHotReload()
	{
		foreach (var player in Core.PlayerManager.GetAllPlayers())
		{
			if (player.IsValid && !player.IsFakeClient)
				Task.Run(() => PlayerData.LoadPlayerDataAsync(player));
		}
	}

	/* ==================== Configuration Loading ==================== */

	private void InitializeConfigs()
	{
		Config = BuildConfigService<PluginConfig>("config.json", "K4Ranks");
		Points = BuildConfigService<PointsConfig>("points.json", "K4RanksPoints");
		RanksConfig = BuildConfigService<RanksConfig>("ranks.json", "K4RanksRanks");
		Commands = BuildConfigService<CommandsConfig>("commands.json", "K4RanksCommands");
		Modules = BuildConfigService<ModuleConfig>("modules.json", "K4RanksModules");
	}

	private IOptionsMonitor<T> BuildConfigService<T>(string fileName, string sectionName) where T : class, new()
	{
		Core.Configuration
			.InitializeJsonWithModel<T>(fileName, sectionName)
			.Configure(cfg => cfg.AddJsonFile(Core.Configuration.GetConfigPath(fileName), optional: false, reloadOnChange: true));

		ServiceCollection services = new();
		services.AddSwiftly(Core)
			.AddOptions<T>()
			.BindConfiguration(sectionName);

		var provider = services.BuildServiceProvider();
		return provider.GetRequiredService<IOptionsMonitor<T>>();
	}

	/* ==================== Shared API ==================== */

	public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
	{
		const string apiVersion = "K4LevelRanks.Api.v1";

		var apiService = new K4RanksApiService(this);
		interfaceManager.AddSharedInterface<IK4RanksApi, K4RanksApiService>(apiVersion, apiService);

		Core.Logger.LogInformation("Shared API registered: {Version}", apiVersion);
	}

	/* ==================== Unload ==================== */

	public override void Unload()
	{
		Scoreboard.Stop();

		Task.Run(async () => await PlayerData.SaveAllPlayersAsync()).Wait();

		WeaponCache.Reset();
	}
}
