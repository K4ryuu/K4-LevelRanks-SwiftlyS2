using Microsoft.Extensions.Logging;

namespace K4Ranks;

public sealed partial class Plugin
{
	public sealed class RankService(RanksConfig ranksConfig)
	{
		/* ==================== Properties ==================== */

		public IReadOnlyList<Rank> Ranks => ranksConfig.Ranks;

		/* ==================== Initialization ==================== */

		public void LoadRanks()
		{
			if (Ranks.Count > 0)
				return;

			if (ranksConfig.Ranks.Count == 0)
			{
				Core.Logger.LogWarning("No ranks configured in ranks.json! Players will have no ranks.");
				return;
			}

			FilterDuplicateRanks();
			SortRanksByPoints();

			Core.Logger.LogInformation("Loaded {Count} unique ranks from config", ranksConfig.Ranks.Count);
		}

		private void FilterDuplicateRanks()
		{
			var uniqueRanks = new List<Rank>();
			var seenRankNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var rank in ranksConfig.Ranks)
			{
				if (seenRankNames.Add(rank.Name))
				{
					uniqueRanks.Add(rank);
				}
				else
				{
					Core.Logger.LogWarning(
						"Duplicate rank name '{RankName}' found in ranks.json. Skipping duplicate.",
						rank.Name
					);
				}
			}

			ranksConfig.Ranks = uniqueRanks;
		}

		private void SortRanksByPoints()
		{
			ranksConfig.Ranks.Sort((a, b) => a.Points.CompareTo(b.Points));
		}

		/* ==================== Rank Lookup ==================== */

		public Rank GetRank(int points)
		{
			if (ranksConfig.Ranks.Count == 0)
				return new Rank { Name = "Unknown", Tag = "[?]", Color = "WHITE", Points = 0 };

			Rank currentRank = ranksConfig.Ranks[0];

			foreach (var rank in ranksConfig.Ranks)
			{
				if (points >= rank.Points)
					currentRank = rank;
				else
					break;
			}

			return currentRank;
		}

		/// <summary>
		/// Gets the 1-based rank index (ID) for scoreboard display
		/// </summary>
		public int GetRankId(int points)
		{
			if (ranksConfig.Ranks.Count == 0)
				return 0;

			int rankId = 1;

			for (int i = 0; i < ranksConfig.Ranks.Count; i++)
			{
				if (points >= ranksConfig.Ranks[i].Points)
					rankId = i + 1;
				else
					break;
			}

			return rankId;
		}

		public Rank? GetNextRank(int points)
		{
			foreach (var rank in ranksConfig.Ranks)
			{
				if (rank.Points > points)
					return rank;
			}

			return null; // Already at max rank
		}
	}
}
