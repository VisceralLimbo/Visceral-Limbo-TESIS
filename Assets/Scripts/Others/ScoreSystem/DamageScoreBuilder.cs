using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageScoreBuilder 
{
    /// <summary>
    /// calculo de DamageScore.
    /// </summary>
    /// <param name="BaseData"></param>
    /// <param name="Victim"></param>
    /// <param name="currenthealth"></param>
    /// <param name="maxhealth"></param>
    /// <returns></returns>
    public static DamageScore Complete(DamageScore BaseData, PlayerContext Victim,float currenthealth,float maxhealth)
    {
        var Score = BaseData;

        Score.Victim = Victim;
        Score.EnemyScoreBase = Victim.EnemyValueScore;

        //enemigo muerto - default
        Score.AddTag(ScoreFlags.None);

        //la victima esta en el aire
        if(Score.Victim.KCCMotor != null && !Score.Victim.KCCMotor.GroundingStatus.IsStableOnGround)
        {
            Score.AddTag(ScoreFlags.Airkill);
        }
        else if(Score.Attacker.KCCMotor != null && !Score.Attacker.KCCMotor.GroundingStatus.IsStableOnGround)
        {
            Score.AddTag(ScoreFlags.Airkill);
        }

        //la victima sufrio un overkill
        if(currenthealth < -maxhealth * 0.25f)
        {
            Score.Overkill = -currenthealth;
            Score.AddTag(ScoreFlags.Overkill);
        }

        if(Score.Victim.faction == Score.Attacker.faction)
        {
            Score.IsFriendlyFire= true;
            Score.AddTag(ScoreFlags.Friendlyfire);
        }

        if(Score.Attacker.faction == FactionID.LimboTrap)
        {
            Score.AddTag(ScoreFlags.TrapKill);
        }

        //añadir aqui futuros tags

        return Score;
    }



}
