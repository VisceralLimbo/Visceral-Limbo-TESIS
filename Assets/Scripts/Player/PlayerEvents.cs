using System;

public static class PlayerEvents
{
    public static event Action OnAttack;
    public static event Action OnDash;
    public static event Action OnParry;
    public static event Action OnWhirlwind;

    public static void Attack() => OnAttack?.Invoke();
    public static void Dash() => OnDash?.Invoke();
    public static void Parry() => OnParry?.Invoke();
    public static void Whirlwind() => OnWhirlwind?.Invoke();
}