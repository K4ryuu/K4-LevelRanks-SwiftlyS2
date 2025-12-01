# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
