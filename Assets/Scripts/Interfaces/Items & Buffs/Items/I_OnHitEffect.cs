using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_OnHitItem
{
    public void OnProcEffect(PlayerContext Inflictor,DamageScore DMS,Health_Component VictimHP);


}
