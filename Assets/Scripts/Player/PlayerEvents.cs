using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action OnAttack;
    public static event Action OnPlayerUsedUtilitySkill;
    public static event Action OnPlayerUsedSkill2;
    public static event Action OnPlayerUsedSkill1;

    public static event Action OnInteractSeeing;

    public static event Action OnInteractStopSeeing;

    public static event Action OnInteract;
    public static event Action OnPlayerSuccesfulHit;

    public static void Attack() => OnAttack?.Invoke();
    public static void PlayerUsedUtilSkill() => OnPlayerUsedUtilitySkill?.Invoke();
    public static void PlayerUsedSkill2() => OnPlayerUsedSkill2?.Invoke();
    public static void PlayerUsedSkill1() => OnPlayerUsedSkill1?.Invoke();

    public static void PlayerSucessfulHit() => OnPlayerSuccesfulHit?.Invoke();

    public static void Interact()
    {
        OnInteract?.Invoke();
      
    }

    public static void InteractSeeing()
    {
        OnInteractSeeing?.Invoke();

    }


    public static void InteractStopSeeing() 
    {
        OnInteractStopSeeing?.Invoke();
      
    }
}