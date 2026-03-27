using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Block : MonoBehaviour
{
    [SerializeField] StatsManager _StatMan;
    [SerializeField] StatIdentifier _StatIdentifier;
    [SerializeField] PlayerContext _Context;
    [SerializeField] StatModifierFloat _FloatStat;

                     private bool StartedBlocking,StoppedBlocking;
    [SerializeField] private int BlockPotency;

    [SerializeField] private bool _isCurrentlyBlocking = false; // Variable para control de estado


    private void Awake()
    {

        if (_Context == null)
        {
            _Context = GetComponent<PlayerContext>();
            if (_Context == null)
            {
                    _Context = GetComponentInChildren<PlayerContext>();
            }
        }
        if (_StatMan == null)
        {
            _StatMan = _Context.Stats;
        }

    }
   

    private void Update()
    {
        // Usamos las propiedades de Input para detectar el MOMENTO del cambio
        bool isPressing = Player_InputHandler.instance.CurrentMovementInput.SustainedRightMouseClick;
        bool released = Player_InputHandler.instance.CurrentMovementInput.ReleasedRightMouseClick;

        if (isPressing && !_isCurrentlyBlocking)
        {
            BlockLogic();
        }
        else if (released && _isCurrentlyBlocking)
        {
            ReleaseLogic();
        }
    }

    private void BlockLogic()
    {
        // Eliminamos el Check del diccionario porque la Stat SIEMPRE existe, 
        // lo que queremos es agregarle un modificador.
        _StatMan.UpdateFloatStatValue(_StatIdentifier, _FloatStat);
        _isCurrentlyBlocking = true;
        Debug.Log("Defensa aumentada");
    }

    private void ReleaseLogic()
    {
        _StatMan.RemoveFloatStatModifier(_StatIdentifier, _FloatStat.EffectName);
        _isCurrentlyBlocking = false;
        Debug.Log("Defensa normalizada");
    }

}
