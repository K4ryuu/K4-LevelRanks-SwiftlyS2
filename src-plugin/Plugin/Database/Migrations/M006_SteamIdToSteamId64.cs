using System.Data;
using FluentMigrator;
using MySqlConnector;
using Npgsql;

namespace K4Ranks.Database.Migrations;

/// <summary>
/// Converts steam ID keys from STEAM_X:Y:Z format to SteamID64 string format
/// across all lvl_base tables.
/// </summary>
[Migration(20251201006)]
public class M006_SteamIdToSteamId64 : Migration
{
	// Table name → steam column name
	private static readonly (string Table, string Column)[] Tables =
	[
		("lvl_base",          "steam"),
		("lvl_base_settings", "steam"),
		("lvl_base_weapons",  "steam"),
		("lvl_base_hits",     "SteamID"),
	];

	public override void Up()
	{
		Execute.WithConnection((connection, transaction) =>
		{
			bool isMySql   = connection is MySqlConnection;
			bool isPostgres = connection is NpgsqlConnection;

			foreach (var (table, column) in Tables)
			{
				if (!TableExists(connection, transaction, table, isMySql, isPostgres))
					continue;

				var sql = BuildConvertSql(table, column, isMySql, isPostgres);

				using var cmd = connection.CreateCommand();
				cmd.Transaction = transaction;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		});
	}

	public override void Down()
	{
		// Conversion back to STEAM_X:Y:Z is not feasible in pure SQL.
		// Down migration is intentionally left empty.
	}

	private static bool TableExists(
		IDbConnection connection,
		IDbTransaction? transaction,
		string table,
		bool isMySql,
		bool isPostgres)
	{
		string sql = isMySql || isPostgres
			? $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{table}'"
			: $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{table}'";

		using var cmd = connection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = sql;

		return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
	}

	/// <summary>
	/// Builds the UPDATE statement that converts STEAM_X:Y:Z values to SteamID64.
	/// Formula: SteamID64 = 76561197960265728 + Z*2 + Y
	///   where STEAM_X:Y:Z — Y is auth bit, Z is account number.
	/// Only rows whose column still matches the STEAM_ prefix are touched.
	/// </summary>
	private static string BuildConvertSql(string table, string column, bool isMySql, bool isPostgres)
	{
		if (isMySql)
		{
			// SUBSTRING_INDEX is MySQL-specific
			return
				$"UPDATE `{table}` " +
				$"SET `{column}` = CAST(" +
				$"  76561197960265728 " +
				$"  + CAST(SUBSTRING_INDEX(`{column}`, ':', -1) AS UNSIGNED) * 2 " +
				$"  + CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(`{column}`, ':', 2), ':', -1) AS UNSIGNED) " +
				$"  AS CHAR) " +
				$"WHERE `{column}` LIKE 'STEAM_%'";
		}

		if (isPostgres)
		{
			// split_part is PostgreSQL-specific
			return
				$"UPDATE \"{table}\" " +
				$"SET \"{column}\" = (" +
				$"  76561197960265728 " +
				$"  + CAST(split_part(\"{column}\", ':', 3) AS BIGINT) * 2 " +
				$"  + CAST(split_part(\"{column}\", ':', 2) AS BIGINT)" +
				$")::TEXT " +
				$"WHERE \"{column}\" LIKE 'STEAM_%'";
		}

		// SQLite — use nested INSTR/SUBSTR
		// For 'STEAM_X:Y:Z':
		//   c1  = position of first ':'
		//   c2r = position of second ':' relative to the substring after c1
		//   Y   = character(s) between c1 and c2 relative to start
		//   Z   = everything after c2
		return
			$"UPDATE \"{table}\" " +
			$"SET \"{column}\" = CAST(" +
			$"  76561197960265728 " +
			$"  + CAST(SUBSTR(\"{column}\", " +
			$"      INSTR(\"{column}\", ':') + " +
			$"      INSTR(SUBSTR(\"{column}\", INSTR(\"{column}\", ':') + 1), ':') + 1) AS INTEGER) * 2 " +
			$"  + CAST(SUBSTR(" +
			$"      SUBSTR(\"{column}\", INSTR(\"{column}\", ':') + 1), " +
			$"      1, " +
			$"      INSTR(SUBSTR(\"{column}\", INSTR(\"{column}\", ':') + 1), ':') - 1) AS INTEGER) " +
			$"  AS TEXT) " +
			$"WHERE \"{column}\" LIKE 'STEAM_%'";
	}
}
