public enum ElementType
{
    Physical,
    Fire,
    Magic,
}


[System.Serializable]
public struct DamageScore
{
    public PlayerContext Attacker;
    public PlayerContext Victim;
    public float DamageAmount;
    public float EnemyScoreBase;
    public float Overkill;
    public bool IsFriendlyFire;
    public bool IsAirBorneKill;

    public ElementType ElementalDamage;
    public FactionID FactionID;

    public ScoreFlags ScoreTags;

    public bool IsTagged(ScoreFlags flags) => (ScoreTags & flags) != 0;

    public void AddTag(ScoreFlags Flags) => ScoreTags |= Flags;


}
///
///  Script creado por Patricio Malvasio Maddalena 2/5/2025
///  
/// este script será usado para calcular el score de la kill del jugador
/// y pasar info del ataque letal de un enemigo
///
///