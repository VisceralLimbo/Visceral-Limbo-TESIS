using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateEnergyCost
{
    public abstract string GetTransitionKey();
    public abstract void SetCost(float NewCost);
    public abstract int GetCost();

    public BaseState GetState();
}
