# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v1.0.3] yee

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
