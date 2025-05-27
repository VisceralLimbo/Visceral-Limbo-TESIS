using System.Collections.Generic;
using UnityEngine;

public class RoomLightController : MonoBehaviour
{
    [SerializeField] private List<Light> lightsToControl;
    [SerializeField] private RoomSpawnerManager spawnerManager;

    private void Start()
    {
        if (spawnerManager != null)
        {
            spawnerManager.OnCombatEnded += HandleCombatEnded;
        }

        SetLightsState(false);
    }

    private void HandleCombatEnded()
    {
        SetLightsState(true);
    }

    private void SetLightsState(bool state)
    {
        foreach (var light in lightsToControl)
        {
            if (light != null)
                light.enabled = state;
        }
    }

    private void OnDestroy()
    {
        if (spawnerManager != null)
        {
            spawnerManager.OnCombatEnded -= HandleCombatEnded;
        }
    }
}

