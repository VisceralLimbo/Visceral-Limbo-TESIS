using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Visceral_Limbo/General/StatSystem/StatBlockScriptableObject")]
public class StatBlockData : ScriptableObject
{
   public List<FloatStat> Stats = new List<FloatStat>();


}
