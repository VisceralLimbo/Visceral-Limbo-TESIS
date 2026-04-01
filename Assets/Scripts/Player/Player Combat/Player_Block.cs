using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Block : MonoBehaviour
{
    [Header("References")]
    [SerializeField] StatsManager _StatMan;
    [SerializeField] StatIdentifier _StatIdentifier;
    [SerializeField] PlayerContext _Context;
    [SerializeField] StatModifierFloat _FloatStat;
    [SerializeField] AnimatorHandler _AnimHandler;

    [Header("Variables")]
    [SerializeField] private int BlockPotency;

    [SerializeField] private bool _isCurrentlyBlocking = false; // Variable para control de estado
    [SerializeField] private string _AnimatorParamID = "IsDefending"; 
    [SerializeField] private string _AnimatorKey = "PlayerWeapon";


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

        if(_AnimHandler == null)
        {
            _AnimHandler = GetComponent<AnimatorHandler>();
            if(_AnimHandler == null)
            {
                _AnimHandler = GetComponentInChildren<AnimatorHandler>();
            }
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

        if(_AnimHandler != null)
        {
            _AnimHandler.SetParameter(_AnimatorKey, _AnimatorParamID
                        , AnimatorControllerParameterType.Bool, true);
        }

    }

    private void ReleaseLogic()
    {
        _StatMan.RemoveFloatStatModifier(_StatIdentifier, _FloatStat.EffectName);
        _isCurrentlyBlocking = false;

        if(_AnimHandler != null)
        {
            _AnimHandler.SetParameter(_AnimatorKey, _AnimatorParamID
                       , AnimatorControllerParameterType.Bool, false);
        }

    }

}
