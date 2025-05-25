using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

public static class KillFeedManager
{

    /// <summary>
    /// Extraemos los flags activos y devolvemos un Ienumerable,porque el 
    /// flag system de unity no posee extension enumerable
    /// </summary>
    /// <param name="Flags">los flags que queremos pasar </param>
    /// <returns></returns>
    public static IEnumerable<ScoreFlags> GetActiveFlags(ScoreFlags Flags)
    {

        foreach(ScoreFlags flag in Enum.GetValues(typeof(ScoreFlags)))
        {
            // no hay flags
            if (flag == ScoreFlags.None) continue;
            if ((Flags & flag) == flag)
            yield return flag;
        }
    }

    /// <summary>
    /// obtener el texto de cada flag
    /// </summary>
    /// <param name="Flags"></param>
    /// <returns></returns>
    public static string GetFlagText(ScoreFlags Flags)
    {
        return Flags switch
        {
            ScoreFlags.Overkill => "+OVERKILL!",
            ScoreFlags.Airkill => "+ASCENDED!",
            ScoreFlags.TrapKill => "+TRAPPED!",
            ScoreFlags.Friendlyfire => "FRIENDLY FIRE!",
            ScoreFlags.None => "+KILL!",
            _ => "shit, not implemented, developers are lazy - limbo",
        };
    }


    /// <summary>
    /// Diccionario de combos posibles en el juego, esto va a escalar horriblemente xd
    /// </summary>
    private static Dictionary<ScoreFlags, string> ComboDictionary = new()
    {
        {ScoreFlags.Friendlyfire | ScoreFlags.Overkill, "<Color=blue> NOT SO BLUE NOW!</Color>" },
        {ScoreFlags.Airkill | ScoreFlags.TrapKill, "<Color=red> FLY SWATTER! </Color>" },

    };


    /// <summary>
    /// componenr el killFeedText
    /// </summary>
    /// <param name="DmScore"></param>
    /// <returns></returns>
    public static string ComposeKillFeedText(DamageScore DmScore)
    {
        var Result = new StringBuilder();


        //chequear si tenemos un combo
        foreach(var Combo in ComboDictionary)
        {
            if((DmScore.ScoreTags & Combo.Key) == Combo.Key)
            {
                Result.Append("_" + Combo.Key);
                return Result.ToString();
            }

        }

        //no hay combos posibles
        foreach(var flag in GetActiveFlags(DmScore.ScoreTags))
        {
            Result.Append("_" + GetFlagText(flag));

        }

        return Result.ToString();

    }


}
