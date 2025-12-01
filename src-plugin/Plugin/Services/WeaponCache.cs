using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Helpers;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace K4Ranks;

public sealed partial class Plugin
{
	/// <summary>
	/// Static weapon cache with lazy initialization at map load.
	/// Caches weapon data from game to avoid hardcoded lists.
	/// </summary>
	public static class WeaponCache
	{
		/* ==================== Fields ==================== */

		private static Dictionary<CSWeaponType, List<WeaponInfo>>? _weaponsByType;
		private static Dictionary<ItemDefinitionIndex, WeaponInfo>? _weaponsByIndex;
		private static Dictionary<string, WeaponInfo>? _weaponsByClassname;

		/* ==================== Properties ==================== */

		private static bool IsInitialized => _weaponsByIndex?.Count > 0;

		/* ==================== Types ==================== */

		public sealed class WeaponInfo
		{
			public required ItemDefinitionIndex Index { get; init; }
			public required string Classname { get; init; }
			public required string DisplayName { get; init; }
			public required CSWeaponType Type { get; init; }
			public required bool IsPrimary { get; init; }
			public required bool IsSecondary { get; init; }
		}

		/* ==================== Initialization ==================== */

		public static void Initialize()
		{
			if (IsInitialized)
				return;

			_weaponsByType = [];
			_weaponsByIndex = [];
			_weaponsByClassname = [];

			foreach (var type in Enum.GetValues<CSWeaponType>())
				_weaponsByType[type] = [];

			foreach (var index in Enum.GetValues<ItemDefinitionIndex>())
				ProcessWeaponIndex(index);

			if (_weaponsByIndex.Count > 0)
				Core.Logger.LogInformation("WeaponCache initialized: {Count} weapons cached", _weaponsByIndex.Count);
		}

		private static void ProcessWeaponIndex(ItemDefinitionIndex index)
		{
			var vdata = Core.Helpers.GetWeaponCSDataFromKey(index);
			if (vdata == null)
				return;

			var enumName = index.ToString();
			var classname = $"weapon_{ToSnakeCase(enumName)}";
			var type = vdata.WeaponType;

			var isPrimary = type is
				CSWeaponType.WEAPONTYPE_RIFLE or
				CSWeaponType.WEAPONTYPE_SNIPER_RIFLE or
				CSWeaponType.WEAPONTYPE_SHOTGUN or
				CSWeaponType.WEAPONTYPE_SUBMACHINEGUN or
				CSWeaponType.WEAPONTYPE_MACHINEGUN;

			var info = new WeaponInfo
			{
				Index = index,
				Classname = classname,
				DisplayName = enumName,
				Type = type,
				IsPrimary = isPrimary,
				IsSecondary = type == CSWeaponType.WEAPONTYPE_PISTOL
			};

			_weaponsByIndex![index] = info;
			_weaponsByClassname![classname.ToLowerInvariant()] = info;
			_weaponsByType![type].Add(info);
		}

		public static void Reset()
		{
			_weaponsByType = null;
			_weaponsByIndex = null;
			_weaponsByClassname = null;
		}

		/* ==================== Lookup Methods ==================== */

		public static WeaponInfo? GetByIndex(ItemDefinitionIndex index)
		{
			EnsureInitialized();
			return _weaponsByIndex?.TryGetValue(index, out var info) == true ? info : null;
		}

		public static WeaponInfo? GetByClassname(string? classname)
		{
			if (string.IsNullOrEmpty(classname))
				return null;

			EnsureInitialized();

			var key = classname.StartsWith("weapon_", StringComparison.OrdinalIgnoreCase)
				? classname.ToLowerInvariant()
				: $"weapon_{classname.ToLowerInvariant()}";

			return _weaponsByClassname?.TryGetValue(key, out var info) == true ? info : null;
		}

		public static IReadOnlyList<WeaponInfo> GetByType(CSWeaponType type)
		{
			EnsureInitialized();
			return _weaponsByType?.TryGetValue(type, out var list) == true ? list : [];
		}

		public static IReadOnlyCollection<WeaponInfo> GetAll()
		{
			EnsureInitialized();
			return _weaponsByIndex?.Values ?? (IReadOnlyCollection<WeaponInfo>)[];
		}

		public static IEnumerable<WeaponInfo> GetAllPrimaries()
		{
			EnsureInitialized();
			return _weaponsByIndex?.Values.Where(w => w.IsPrimary) ?? [];
		}

		public static IEnumerable<WeaponInfo> GetAllSecondaries()
		{
			EnsureInitialized();
			return _weaponsByIndex?.Values.Where(w => w.IsSecondary) ?? [];
		}

		/* ==================== Parsing Helpers ==================== */

		public static ItemDefinitionIndex? ParseWeapon(string? weaponName)
		{
			var info = GetByClassname(weaponName);
			return info?.Index;
		}

		public static CSWeaponType GetWeaponType(string? weaponName)
		{
			var info = GetByClassname(weaponName);
			return info?.Type ?? CSWeaponType.WEAPONTYPE_UNKNOWN;
		}

		/* ==================== Private Helpers ==================== */

		private static string ToSnakeCase(string name)
		{
			var result = new System.Text.StringBuilder();

			for (int i = 0; i < name.Length; i++)
			{
				var c = name[i];
				if (char.IsUpper(c) && i > 0)
					result.Append('_');
				result.Append(char.ToLowerInvariant(c));
			}

			return result.ToString();
		}

		private static void EnsureInitialized()
		{
			if (!IsInitialized)
			{
				Core.Logger.LogWarning("WeaponCache accessed before initialization, initializing now");
				Initialize();
			}
		}
	}
}
