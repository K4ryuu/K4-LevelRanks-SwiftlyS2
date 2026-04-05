using FluentMigrator;

namespace K4Ranks.Database.Migrations;

/// <summary>
/// Converts the 'name' column in lvl_base to utf8mb4 on MySQL/MariaDB.
/// MySQL's default 'utf8' charset is actually utf8mb3, which only supports
/// Basic Multilingual Plane characters (≤ U+FFFF). Player names containing
/// characters outside the BMP — such as Mathematical Fraktur, emoji, or other
/// supplementary Unicode — require utf8mb4 (true 4-byte UTF-8).
/// PostgreSQL and SQLite already store all Unicode correctly, so no change is
/// needed on those engines.
/// </summary>
[Migration(20251201005)]
public class M005_NameColumnUtf8mb4 : Migration
{
	public override void Up()
	{
		IfDatabase("MySql5").Execute.Sql(
			"ALTER TABLE `lvl_base` MODIFY COLUMN `name` VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '';"
		);
	}

	public override void Down()
	{
		IfDatabase("MySql5").Execute.Sql(
			"ALTER TABLE `lvl_base` MODIFY COLUMN `name` VARCHAR(128) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '';"
		);
	}
}
