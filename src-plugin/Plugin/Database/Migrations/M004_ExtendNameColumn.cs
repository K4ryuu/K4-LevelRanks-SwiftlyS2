using FluentMigrator;

namespace K4Ranks.Database.Migrations;

/// <summary>
/// Extends the 'name' column from VARCHAR(64) to VARCHAR(128).
/// ASCII art player names frequently exceed the original 64-character limit,
/// causing the INSERT to fail and the player's rank to never be persisted.
/// </summary>
[Migration(20251201004)]
public class M004_ExtendNameColumn : Migration
{
	public override void Up()
	{
		Alter.Table("lvl_base")
			.AlterColumn("name").AsString(128).NotNullable().WithDefaultValue("");
	}

	public override void Down()
	{
		Alter.Table("lvl_base")
			.AlterColumn("name").AsString(64).NotNullable().WithDefaultValue("");
	}
}
