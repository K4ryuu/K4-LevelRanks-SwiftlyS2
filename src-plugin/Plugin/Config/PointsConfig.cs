namespace K4Ranks;

/// <summary>
/// Point values configuration (points.json)
/// </summary>
public sealed class PointsConfig
{
	/* ==================== Combat ==================== */

	public int Kill { get; set; } = 8;
	public int Death { get; set; } = -5;
	public int Headshot { get; set; } = 5;
	public int Assist { get; set; } = 5;
	public int AssistFlash { get; set; } = 7;
	public int TeamKill { get; set; } = -10;
	public int TeamAssist { get; set; } = -5;
	public int TeamAssistFlash { get; set; } = -7;
	public int Suicide { get; set; } = -5;

	/* ==================== Special Kills ==================== */

	public int NoScope { get; set; } = 15;
	public int Thrusmoke { get; set; } = 15;
	public int BlindKill { get; set; } = 5;
	public int Penetrated { get; set; } = 3;
	public int LongDistanceKill { get; set; } = 8;
	public int LongDistance { get; set; } = 30;

	/* ==================== Weapon Kills ==================== */

	public int KnifeKill { get; set; } = 15;
	public int TaserKill { get; set; } = 20;
	public int GrenadeKill { get; set; } = 30;
	public int InfernoKill { get; set; } = 30;
	public int ImpactKill { get; set; } = 100;

	/* ==================== Round ==================== */

	public int RoundWin { get; set; } = 5;
	public int RoundLose { get; set; } = -2;
	public int MVP { get; set; } = 10;

	/* ==================== Bomb ==================== */

	public int BombPlant { get; set; } = 10;
	public int BombDefuse { get; set; } = 10;
	public int BombDefuseOthers { get; set; } = 3;
	public int BombExploded { get; set; } = 10;
	public int BombPickup { get; set; } = 2;
	public int BombDrop { get; set; } = -2;

	/* ==================== Hostage ==================== */

	public int HostageRescue { get; set; } = 15;
	public int HostageRescueAll { get; set; } = 10;
	public int HostageHurt { get; set; } = -2;
	public int HostageKill { get; set; } = -20;

	/* ==================== Playtime ==================== */

	public int PlaytimePoints { get; set; } = 10;
	public float PlaytimeMinutes { get; set; } = 5.0f;

	/* ==================== Killstreaks ==================== */

	public int SecondsBetweenKills { get; set; } = 0;
	public int DoubleKill { get; set; } = 5;
	public int TripleKill { get; set; } = 10;
	public int Domination { get; set; } = 15;
	public int Rampage { get; set; } = 20;
	public int MegaKill { get; set; } = 25;
	public int Ownage { get; set; } = 30;
	public int UltraKill { get; set; } = 35;
	public int KillingSpree { get; set; } = 40;
	public int MonsterKill { get; set; } = 45;
	public int Unstoppable { get; set; } = 50;
	public int GodLike { get; set; } = 60;
}
