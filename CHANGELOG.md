# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v1.1.2]

### Fixed

- **NullReferenceException fix**: Fixed crashes in player_hurt and player_death event handlers
  - Added null-conditional operators to `@event.Accessor?.GetPlayer()` calls
  - Added explicit null checks before accessing player properties in kill type determination
  - Prevents crashes when event accessor or player objects are null during edge cases (player disconnects, etc.) or world damages

## [v1.1.1]

### Fixed

- **Critical issue fix**: Fixed `ObjectDisposedException` when players disconnect
  - Migrated from game events (`EventPlayerActivate`, `EventPlayerDisconnect`) to SwiftlyS2 API events
  - Now using `Core.Event.OnClientPutInServer` for player connect handling
  - Now using `Core.Event.OnClientDisconnected` for player disconnect handling
  - API events fire before player object disposal, preventing unobserved task exceptions

## [v1.1.0]

### Added

- **Playtime commands** ([#3](https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2/issues/3)):
  - `!mytime` (aliases: `playtime`, `servertime`) - Shows player's total time on server
  - `!ttop` (aliases: `timetop`, `toptime`) - Shows top players by playtime leaderboard
  - Playtime formatting with localizable units (days, hours, minutes)
- **Position notifications** (requested by Mafel): Top list commands now display player's rank in chat
  - `!top` shows: "You are ranked #X out of Y players"
  - `!ttop` shows: "You are ranked #X out of Y players by playtime"

### Changed

- **Top players menu**: Now displays rank tag alongside player name and points
  - Format: `#1 PlayerName [GN1] - 5000 pts` (using short rank tag instead of full name)

### Fixed

- **Race condition fix** ([#5](https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2/issues/5)): Fixed duplicate entry MySQL error when saving player data concurrently
  - Changed from check-then-act pattern to try-catch INSERT with UPDATE fallback
  - Applied fix to all save operations: PlayerData, PlayerSettings, WeaponStats, HitData
- **Security fix** ([#6](https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2/issues/6)): Added permission enforcement for admin commands
  - Admin commands now require `k4-levelranks.admin` permission
  - Affected commands: `setpoints`, `givepoints`, `removepoints`

## [v1.0.3]

### Changed

- **GameRules handling**: Updated to use `Core.EntitySystem.GetGameRules()` API directly instead of querying entities
- **Config system**: Migrated from `IOptions<T>` to `IOptionsMonitor<T>` for reactive config updates
  - All config values now accessed via `.CurrentValue` property
  - Configs can be reloaded at runtime without server restart
- **Config registration**: Simplified config initialization to use `AddOptions<T>()` with `BindConfiguration()`

### Technical

- Refactored Plugin.cs to use simplified GameRules accessor
- Updated all config accesses across Events, Commands, Services to use `CurrentValue` pattern
- Configuration now supports live-reload via SwiftlyS2's `reloadOnChange: true` setting

## [v1.0.2]

### Fixed

- Fixed new players not being saved to database (INSERT missing primary key due to Dommel assuming auto-generated keys)
- Added `[DatabaseGenerated(DatabaseGeneratedOption.None)]` attribute to all model primary keys (PlayerData, PlayerSettings, WeaponStatRecord, HitData)

## [v1.0.1]

### Added

- **Multi-database support**: Now supports MySQL/MariaDB, PostgreSQL, and SQLite
- **Database migrations**: Automatic schema management with FluentMigrator
- **ORM integration**: Dapper + Dommel for type-safe database operations

### Changed

- Refactored database layer to use Dommel ORM instead of raw SQL queries
- Improved database compatibility across different database engines
- Optimized publish output by excluding unused language resources and database providers

### Fixed

- Fixed SQL syntax compatibility issues with different MySQL/MariaDB versions
