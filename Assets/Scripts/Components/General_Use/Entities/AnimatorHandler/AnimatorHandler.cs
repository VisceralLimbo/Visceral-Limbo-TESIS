using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq;

public class AnimatorHandler : MonoBehaviour
{
    public List<AnimatorEntry> AnimatorsData = new List<AnimatorEntry>();

    Dictionary<string,AnimatorEntry> AnimatorDictionary= new Dictionary<string,AnimatorEntry>();

    private void Awake()
    {
        foreach (var entry in AnimatorsData)
        {
            if(entry.AnimatorController== null)
            {
                Debug.LogError("[Visceral Error] animator handler:null animator value from entry: " + entry.ID);
                continue;

            }

            if (AnimatorDictionary.ContainsKey(entry.ID)) 
            {
                Debug.LogWarning("<Color=Color.lightblue> Visceral warning:" +
                    " the specified key " +entry.ID +" already exist on animator handler: " 
                     + this.gameObject.name +" skipped to avoid any duplicates</Color>");
                                 
                continue;
            }
                

            AnimatorDictionary.Add(entry.ID, entry);

            print("added animator to dictionary" + entry.ToString() + entry.ID);
        }

        TimeDilationManager.OnTimeScaleChanged += ChangeAnimTime;
    }
    
    private void ChangeAnimTime(float time)
    {
        foreach(var animEntry in AnimatorDictionary.Values)
        {
            if(animEntry != null)
            {
                Animator Anim;
                TryGetAnimator(animEntry.ID,out Anim);

                if(Anim != null)
                {
                    Anim.speed = time;
                }
            }
            else
            {
                Debug.LogError("Visceral Error: El Anim no existe" + animEntry.ID);
            }
        }
    }


    /// <summary>
    /// esta funcion devuelve un bool (y opcionalmente el valor) si existe el animator
    /// bajo la llave indicada
    /// </summary>
    /// <param name="Animatorkey">la llave del animator usada para suscribirlo al diccionario interno</param>
    /// <param name="animator">el animator que se estaba buscando</param>
    /// <returns></returns>
    public bool TryGetAnimator(string Animatorkey, out Animator animator)
    {
        if(!isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            animator = null;
            return false;
        }

        animator = null;

        if (AnimatorDictionary.TryGetValue(Animatorkey,out AnimatorEntry ANIM))
        {
            animator = ANIM.AnimatorController;
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// funcion para setear un parametro deseado
    /// </summary>
    /// <param name="Animatorkey">llave del animator </param>
    /// <param name="parameterName">nombre del parametro (RECORDATORIO: CUIDADO CON STRINGS!)</param>
    /// <param name="Type">enum del tipo de parametro, necesario para castear al evento correcto</param>
    /// <param name="value"> valor que se quiere settear</param>
    public void SetParameter(string Animatorkey,string parameterName,AnimatorControllerParameterType Type, object value = null)
    {
        if (!isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            return;
        }

        if (!AnimatorDictionary.ContainsKey(Animatorkey))
        {
            Debug.LogError("<Color=Color.blue> Visceral Error: the specified key:" + Animatorkey +
                           " doesnt exist on " + this.name + " please check key</Color>");
            return;
        }

        Animator ANIM = AnimatorDictionary[Animatorkey].AnimatorController;

        if (!HasParameter(ANIM, parameterName) )
        {
            Debug.LogError("<Color=Color.blue> Visceral Error: the specified parameter:" + parameterName +
                          " doesnt exist on the animator:  " + ANIM.name + " please check animator</Color>");
            return;
        }


        switch (Type)
        {
            case AnimatorControllerParameterType.Float:
               if((value is float || value is int)) ANIM.SetFloat(parameterName, (float)value);
                else
                {
                    Debug.LogError("[Visceral Error]: the specified parameter:" + parameterName +
                          " Isnt of type Float");
                }
                break;
            case AnimatorControllerParameterType.Bool: 
                if(value is bool) ANIM.SetBool(parameterName,(bool)value);
                else
                {
                    Debug.LogError("[Visceral Error]: the specified parameter:" + parameterName +
                          " Isnt of type Bool");
                }
                break;
            case AnimatorControllerParameterType.Int:
                if(value is int) ANIM.SetInteger(parameterName,(int)value);
                else
                {
                    Debug.LogError("[Visceral Error]: the specified parameter:" + parameterName + " Isnt of type Int");
                }

                break;
            case AnimatorControllerParameterType.Trigger:
                    ANIM.SetTrigger(parameterName);

                break;
        }
    }

    /// <summary>
    /// funcion para resetear un trigger deseado
    /// </summary>
    /// <param name="AnimatorKey">llave del animator guardado </param>
    /// <param name="ParameterName">nombre del parametro trigger que se desea resetear</param>
    public void ResetTrigger(string AnimatorKey, string ParameterName)
    {
        if (!isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            return;
        }

        if (AnimatorDictionary.TryGetValue(AnimatorKey,out AnimatorEntry ANIMEntry))
        {
            var ANIM = ANIMEntry.AnimatorController;

            if (HasParameter(ANIM, ParameterName, AnimatorControllerParameterType.Trigger))
            {
                ANIM.ResetTrigger(ParameterName);

            }
        }
        else
        {
            Debug.LogError("< Color = Color.blue > Visceral Error: the specified key:" + AnimatorKey +
                           " doesnt exist on " + this.name + " please check key</Color>");
        }
    }

    public void ResetAllTriggers(string AnimatorKey)
    {
        if (!isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            return;
        }

        if (AnimatorDictionary.TryGetValue(AnimatorKey, out AnimatorEntry ANIMEntry))
        {
            Animator ANIM = ANIMEntry.AnimatorController;

            foreach(var param in ANIM.parameters)
            {
                if(param.type == AnimatorControllerParameterType.Trigger)
                {
                    ANIM.ResetTrigger(param.name);
                }
                
            }
        }
    }


    private bool HasParameter(Animator ANIM, string ParamName, AnimatorControllerParameterType? type = null )
    {
        if (!isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            return false;
        }

        foreach (var param in ANIM.parameters)
        {
            if (param.name == ParamName&&(!type.HasValue ||param.type == type.Value) ) return true;     
        }
        Debug.LogWarning("<Color=Color.lightblue> Visceral warning:" +
                    " the specified animator" + ANIM.name + " doesnt have the parameter: "
                     + ParamName + " of type: " + type.ToString()+ " check animator</Color>");
        return false;
    }

    private void OnDestroy()
    {
        TimeDilationManager.OnTimeScaleChanged -= ChangeAnimTime;
    }

    private void OnDisable()
    {
        TimeDilationManager.OnTimeScaleChanged -= ChangeAnimTime;
    }
}
