using K4RanksSharedApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace K4Ranks;

[PluginMetadata(
	Id = "k4.levelranks",
	Version = "1.0.2",
	Name = "K4 - Level Ranks",
	Author = "K4ryuu",
	Description = "Experience-based ranking system with configurable ranks, detailed player statistics, weapon tracking, and hit analysis for CS2."
)]
public sealed partial class Plugin(ISwiftlyCore core) : BasePlugin(core)
{
	/* ==================== Static Access ==================== */

	public static new ISwiftlyCore Core { get; private set; } = null!;

	/* ==================== Configurations ==================== */

	internal PluginConfig Config { get; private set; } = null!;
	internal PointsConfig Points { get; private set; } = null!;
	internal RanksConfig RanksConfig { get; private set; } = null!;
	internal CommandsConfig Commands { get; private set; } = null!;
	internal ModuleConfig Modules { get; private set; } = null!;

	/* ==================== Services ==================== */

	internal RankService Ranks { get; private set; } = null!;
	internal DatabaseService Database { get; private set; } = null!;
	internal PlayerDataService PlayerData { get; private set; } = null!;
	internal ScoreboardService Scoreboard { get; private set; } = null!;

	/* ==================== Game Rules ==================== */

	private CCSGameRules? GetGameRules()
	{
		var proxy = Core.EntitySystem.GetAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
		return proxy?.GameRules;
	}

	internal bool IsWarmup => GetGameRules()?.WarmupPeriod == true;

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
			Config.Database.Connection,
			Config.Database.PurgeDays,
			Config.Rank.StartPoints,
			Modules
		);

		Ranks = new RankService(RanksConfig);
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
		Config = BuildConfigService<PluginConfig>("config.json", "K4Ranks").Value;
		Points = BuildConfigService<PointsConfig>("points.json", "K4RanksPoints").Value;
		RanksConfig = BuildConfigService<RanksConfig>("ranks.json", "K4RanksRanks").Value;
		Commands = BuildConfigService<CommandsConfig>("commands.json", "K4RanksCommands").Value;
		Modules = BuildConfigService<ModuleConfig>("modules.json", "K4RanksModules").Value;
	}

	private IOptions<T> BuildConfigService<T>(string fileName, string sectionName) where T : class, new()
	{
		Core.Configuration
			.InitializeJsonWithModel<T>(fileName, sectionName)
			.Configure(cfg => cfg.AddJsonFile(Core.Configuration.GetConfigPath(fileName), optional: false, reloadOnChange: true));

		// Setup DI with validation on startup
		ServiceCollection services = new();
		services.AddSwiftly(Core)
			.AddOptionsWithValidateOnStart<T>()
			.BindConfiguration(sectionName);

		// Build and validate
		var provider = services.BuildServiceProvider();
		return provider.GetRequiredService<IOptions<T>>();
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
