<a name="readme-top"></a>

![GitHub tag (with filter)](https://img.shields.io/github/v/tag/K4ryuu/K4-LevelRanks-SwiftlyS2?style=for-the-badge&label=Version)
![GitHub Repo stars](https://img.shields.io/github/stars/K4ryuu/K4-LevelRanks-SwiftlyS2?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/K4ryuu/K4-LevelRanks-SwiftlyS2?style=for-the-badge)
![GitHub](https://img.shields.io/github/license/K4ryuu/K4-LevelRanks-SwiftlyS2?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/K4ryuu/K4-LevelRanks-SwiftlyS2/total?style=for-the-badge)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://dsc.gg/k4-fanbase)

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">KitsuneLab©</h1>
  <h3 align="center">K4-LevelRanks</h3>
  <a align="center">A comprehensive ranking and statistics system for Counter-Strike 2. Features point-based progression, detailed player statistics, weapon tracking, and LVL Ranks database compatibility.</a>

  <p align="center">
    <br />
    <a href="https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2/releases/latest">Download</a>
    ·
    <a href="https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2/issues/new?assignees=K4ryuu&labels=bug&projects=&template=bug_report.md&title=%5BBUG%5D">Report Bug</a>
    ·
    <a href="https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2/issues/new?assignees=K4ryuu&labels=enhancement&projects=&template=feature_request.md&title=%5BREQ%5D">Request Feature</a>
  </p>
</div>

### Support My Work

I create free, open-source Counter-Strike 2 plugins for the community. If you'd like to support my work, consider becoming a sponsor!

#### 💖 GitHub Sponsors

Support this project through [GitHub Sponsors](https://github.com/sponsors/K4ryuu) with flexible options:

- **One-time** or **monthly** contributions
- **Custom amount** - choose what works for you
- **Multiple tiers available** - from basic benefits to priority support or private project access

Every contribution helps me dedicate more time to development, support, and creating new features. Thank you! 🙏

<p align="center">
  <a href="https://github.com/sponsors/K4ryuu">
    <img src="https://img.shields.io/badge/sponsor-30363D?style=for-the-badge&logo=GitHub-Sponsors&logoColor=#EA4AAA" alt="GitHub Sponsors" />
  </a>
</p>

⭐ **Or support me for free by starring this repository!**
---

## Features

### Ranking System

- **Point-based progression** with fully customizable point values
- **Dynamic point multipliers** based on victim/attacker point ratio
- **Customizable ranks** with colors, tags, and point thresholds
- **LVL Ranks database compatible** - works with existing databases
- **Fake competitive ranks** - Premier, Competitive, Wingman, or custom icon ranks

### Statistics Tracking

- **Combat stats**: Kills, deaths, assists, headshots, K/D ratio, accuracy
- **Round stats**: Wins, losses, rounds played, MVP awards
- **Game stats**: Match wins, losses, games played
- **Playtime tracking** with optional point rewards

### Weapon Statistics (Optional Module)

- Per-weapon kills, deaths, headshots
- Shots fired, hits, accuracy per weapon
- Damage dealt per weapon

### Hit Statistics (Optional Module)

- Hitbox/body part tracking (ExStats Hits compatible)
- Damage distribution by body region
- Head, chest, stomach, arms, legs tracking

### Point Events

| Category          | Events                                                                |
| ----------------- | --------------------------------------------------------------------- |
| **Combat**        | Kill, Death, Headshot, Assist, Flash Assist, Team Kill, Suicide       |
| **Special Kills** | No-scope, Through Smoke, Blind Kill, Wallbang, Long Distance          |
| **Weapon Kills**  | Knife, Taser, Grenade, Molotov/Incendiary, Impact (Flash/Smoke/Decoy) |
| **Killstreaks**   | Double Kill → God Like (12 levels)                                    |
| **Objectives**    | Bomb Plant/Defuse/Explode, Hostage Rescue/Hurt/Kill                   |
| **Round**         | Round Win/Lose, MVP                                                   |
| **Playtime**      | Configurable points per X minutes                                     |

### Scoreboard Integration

- **Clan tag ranks** - Show rank in player's clan tag
- **Score sync** - Sync scoreboard score with points
- **Competitive rank display** - Premier, Competitive, Wingman, or custom ranks

### VIP Support

- Point multiplier for VIP players
- Configurable permission flags

### Developer API

- Shared API for other plugins (`K4LevelRanks.Api.v1`)
- Access player data, points, ranks programmatically

---

## Dependencies

- [**SwiftlyS2**](https://github.com/swiftly-solution/swiftlys2): Server plugin framework for Counter-Strike 2
- **Database**: One of the following supported databases:
  - **MySQL / MariaDB** - Recommended for production
  - **PostgreSQL** - Full support
  - **SQLite** - Great for single-server setups

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Installation

1. Install [SwiftlyS2](https://github.com/swiftly-solution/swiftlys2) on your server
2. Configure your database connection in SwiftlyS2's `database.jsonc` (MySQL, PostgreSQL, or SQLite)
3. [Download the latest release](https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2/releases/latest)
4. Extract to your server's `swiftlys2/plugins/` directory
5. Configure the plugin files in `swiftlys2/configs/plugins/k4.levelranks/`
6. Restart your server - database tables will be created automatically

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Configuration Files

### `config.json` - Main Settings

| Section        | Option               | Description                                 | Default                 |
| -------------- | -------------------- | ------------------------------------------- | ----------------------- |
| **Database**   | `Connection`         | Database connection name                    | `"host"`                |
|                | `PurgeDays`          | Days to keep inactive records (0 = forever) | `30`                    |
| **Rank**       | `StartPoints`        | Starting points for new players             | `0`                     |
|                | `MinPlayers`         | Minimum players for points to be awarded    | `4`                     |
|                | `WarmupPoints`       | Allow points during warmup                  | `false`                 |
|                | `PointsForBots`      | Award points for killing bots               | `false`                 |
|                | `FFAMode`            | FFA mode (no team penalties)                | `false`                 |
| **Scoreboard** | `Clantags`           | Show rank in clan tags                      | `true`                  |
|                | `ScoreSync`          | Sync score with points                      | `false`                 |
|                | `UseRanks`           | Show competitive ranks                      | `true`                  |
|                | `RankMode`           | 1=Premier, 2=Competitive, 3=Wingman, 4=DZ   | `1`                     |
| **Points**     | `RoundEndSummary`    | Show summary instead of per-action messages | `false`                 |
|                | `DynamicDeathPoints` | Dynamic multiplier based on point ratio     | `true`                  |
|                | `ShowPlayerNames`    | Show player names in point messages         | `false`                 |
| **VIP**        | `Multiplier`         | Point multiplier for VIP                    | `1.25`                  |
|                | `Flags`              | Permission flags for VIP status             | `["k4-levelranks.vip"]` |

### `points.json` - Point Values

Fully customizable point values for all events. Set to `0` to disable any event.

### `ranks.json` - Rank Configuration

```json
{
  "Ranks": [
    { "Name": "Silver I", "Tag": "[S1]", "Color": "GRAY", "Points": 0 },
    { "Name": "Gold Nova I", "Tag": "[GN1]", "Color": "GOLD", "Points": 1000 },
    { "Name": "Global Elite", "Tag": "[GE]", "Color": "YELLOW", "Points": 5000 }
  ]
}
```

### `modules.json` - Optional Features

| Module               | Description                       | Default |
| -------------------- | --------------------------------- | ------- |
| `WeaponStatsEnabled` | Track per-weapon statistics       | `true`  |
| `HitStatsEnabled`    | Track hitbox/body part statistics | `true`  |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Commands

### Player Commands

| Command           | Aliases                | Description                   |
| ----------------- | ---------------------- | ----------------------------- |
| `!rank`           | `!myrank`              | Open main rank menu           |
| `!ranks`          | `!ranklist`            | View all available ranks      |
| `!top`            | `!ranktop`, `!toplist` | View top players              |
| `!stats`          | `!mystats`, `!stat`    | View detailed statistics      |
| `!weaponstats`    | `!ws`                  | View weapon statistics        |
| `!hitstats`       | `!hs`                  | View hit/body part statistics |
| `!settings`       | `!options`             | Player settings menu          |
| `!resetmyrank`    | -                      | Reset your own rank           |
| `!togglepointmsg` | -                      | Toggle point messages         |

### Admin Commands

| Command                           | Permission            | Description               |
| --------------------------------- | --------------------- | ------------------------- |
| `!setpoints <target> <amount>`    | `k4-levelranks.admin` | Set player's points       |
| `!givepoints <target> <amount>`   | `k4-levelranks.admin` | Give points to player     |
| `!removepoints <target> <amount>` | `k4-levelranks.admin` | Remove points from player |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Database Structure

The plugin uses LVL Ranks compatible database tables:

- `lvl_base` - Main player statistics
- `lvl_base_settings` - Player preferences
- `lvl_base_weapons` - Weapon statistics (optional)
- `lvl_base_hits` - Hit statistics (optional)

### Supported Databases

| Database        | Status  | Notes                                      |
| --------------- | ------- | ------------------------------------------ |
| MySQL / MariaDB | ✅ Full | Recommended for multi-server setups        |
| PostgreSQL      | ✅ Full | Alternative for existing Postgres setups   |
| SQLite          | ✅ Full | Perfect for single-server, no setup needed |

**Migration from LVL Ranks**: Simply point the plugin to your existing MySQL database - no migration needed!

**Automatic Schema Management**: The plugin uses FluentMigrator to automatically create and update database tables. Optional modules (WeaponStats, HitStats) only create their tables when enabled.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## License

Distributed under the GPL-3.0 License. See [`LICENSE.md`](LICENSE.md) for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
