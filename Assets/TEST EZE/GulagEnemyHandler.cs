using UnityEngine;

public class GulagEnemyHandler : MonoBehaviour
{
    private Health_Component _hp;

    void Start()
    {
        _hp = GetComponent<Health_Component>();
        if (_hp == null) _hp = GetComponentInChildren<Health_Component>();

        if (_hp != null)
        {
            // me suscribo a la muerte
            _hp.OnDeath += NotifVictory;
        }
    }

    private void NotifVictory()
    {
        // busco al player para avisar q gano el duelo
        Player_HealthComp player = FindObjectOfType<Player_HealthComp>();
        if (player != null)
        {
            player.VictoryGulag();
        }

        // limpio la suscripcion
        _hp.OnDeath -= NotifVictory;
    }
}
