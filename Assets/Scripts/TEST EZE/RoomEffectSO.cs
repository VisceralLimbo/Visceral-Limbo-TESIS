using UnityEngine;

public abstract class RoomEffectSO : ScriptableObject
{
    public abstract void ApplyEffect(RoomSpawnerManager manager);
    public abstract void RemoveEffect(RoomSpawnerManager manager);

}
