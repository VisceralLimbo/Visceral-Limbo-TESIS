using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ScoreFlags 
{
   None = 0,
   Airkill = 1 << 0,
   Overkill = 1 << 1,
   Friendlyfire = 1 << 2,
   TrapKill = 1 << 3,
   Skill1Kill = 1 << 4,
   Skill2Kill = 1 << 5,
   Parried = 1 << 6,
   Explosion = 1 << 7,

}
