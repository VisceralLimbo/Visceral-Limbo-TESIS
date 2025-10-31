using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action OnAttack;
    public static event Action OnDash;
    public static event Action OnParry;
    public static event Action OnWhirlwind;

    public static event Action OnInteractSeeing;

    public static event Action OnInteractStopSeeing;

    public static event Action OnInteract;

    public static void Attack() => OnAttack?.Invoke();
    public static void Dash() => OnDash?.Invoke();
    public static void Parry() => OnParry?.Invoke();
    public static void Whirlwind() => OnWhirlwind?.Invoke();

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