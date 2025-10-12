using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    /// <summary>
    /// diccionario de buffos activos
    /// </summary>
    Dictionary<string, BuffBehavior> BuffDictionary = new Dictionary<string, BuffBehavior>();

    /// <summary>
    /// diccionario de debuffos activos
    /// </summary>
    Dictionary<string, BuffBehavior> DeBuffDictionary = new Dictionary<string, BuffBehavior>();

    /// <summary>
    /// diccionario de cooldowns de buffos / debuffos
    /// </summary>
    Dictionary<BuffBehavior, float> buffDurationDictionary = new Dictionary<BuffBehavior, float>();

    [Tooltip("Donde se deberian de añadir los componentes de buffs / debuffs, pensarlo como en que carpeta queremos que vayan los buffs")]
    [SerializeField] Transform BuffComponentHierarchy;

    [Tooltip("StatManager de la entidad")]
    [SerializeField] StatsManager StatManager;
    private void Start()
    {
        if(BuffComponentHierarchy == null) BuffComponentHierarchy = this.transform;
    }

    /// <summary>
    /// funcion para añadir un nuevo buff
    /// </summary>
    /// <param name="BuffID"> ID del buff</param>
    /// <param name="NewBuff"> El scriptableObject del buff </param>
    /// <param name="_Buffpotency">la potencia a aplicar del buff </param>
    public void AddNewBuff(string BuffID, BuffSO NewBuff,int _Buffpotency)
    {

        // definimos el data del buffSO para saber que vamos a instanciar.
        // porque addcomponent es una merda que pide el uso directo de types o generics...
        // y no acepta el uso directo de clases :(
        System.Type BuffType = System.Type.GetType(NewBuff.AssemblyQualifiedName);
        if(BuffType == null)
        {
            Debug.LogError("<Color = blue>[Visceral Error]: no se encontro la clase " + NewBuff.AssemblyQualifiedName + "corroborar el scriptable object "+ NewBuff.name + "</color>");
            return;
        }


        // seleccionamos el diccionario a usar
        var targetdictionary = NewBuff.BuffType == BuffTypes.Buff 
                                                    ? BuffDictionary : DeBuffDictionary; 

        // chequeamos si existe la llave en primer lugar
        if(targetdictionary.ContainsKey(BuffID))
        {
            // la llave existe ie: ya tenemos este buff

            //si el nuevo buffo a aplicar tiene que overridear al buff viejo
            if (NewBuff.ShouldOverrideSameBuffs)
            {
                //1) primero vamos a expirar el buffo viejo:
                var LastBuff = targetdictionary[BuffID];

                ExpireBuff(LastBuff, targetdictionary);


                //2) creamos el componente en el hierarchy deseado
                var finalBuff = BuffComponentHierarchy.gameObject.AddComponent(BuffType) as BuffBehavior;

                //3) indicamos que la posicion BuffID es igual al nuevo buffo creado
                targetdictionary[BuffID] = finalBuff;

                //4) ahora vamos a aplicar la potencia deseada
                finalBuff.OnAddPotency(_Buffpotency);

                //5) llamamos al OnApply
                finalBuff.OnApply(this,StatManager);

                if (!NewBuff.IsInfinityDuration)
                {
                    //6) seteamos el duration del buff  
                    buffDurationDictionary[finalBuff] = NewBuff.BuffDuration;
                }

                //7) hack?: como el componente no conoce el SO, se lo enviamos. para solucionar problemas futuros
                finalBuff.SetSO(NewBuff);
            }

            //si el nuevo buffo a aplicar debería en su lugar potenciar al buff viejo.
            else if(NewBuff.ShouldScaleWithMultipleInstances)
            {
                // podemos añadir mas potencia?
                if(targetdictionary[BuffID].GetPotency() < NewBuff.MaxBuffPotency)
                {
                    //sumamos más potencia al buffo actual
                    targetdictionary[BuffID].OnAddPotency(_Buffpotency);

                    if (!NewBuff.IsInfinityDuration)
                    {
                        //reiniciamos duracion
                        buffDurationDictionary[targetdictionary[BuffID]] = NewBuff.BuffDuration;
                    }
                 
                }
                else
                {
                    if (!NewBuff.IsInfinityDuration)
                    {
                        // no sumamos más potencia, pero si le reiniciamos el cooldown
                        buffDurationDictionary[targetdictionary[BuffID]] = NewBuff.BuffDuration;
                    }
                }
            }
            else
            {
                // si el nuevo buff no puede ni overridear ni potenciar al viejo
                // vamos a descartarlo. porque quedaría raro poder tener dos instancias
                // distintas del mismo buffo, no?
                return;
            }
        }
        // el buffo a aplicar no existe.
        else
        {
            //1) añadimos el buff en la posicion deseada
            var finalbuff = BuffComponentHierarchy.gameObject.AddComponent(BuffType) as BuffBehavior;

            //2) guardamos la nueva posicion
            targetdictionary.TryAdd(BuffID, finalbuff);

            //3) añadimos la potencia del buff;
            finalbuff.OnAddPotency(_Buffpotency);

            //4) llamamos al OnApply del nuevo buff
            finalbuff.OnApply(this,StatManager);
            
            // si el buff no es infinito
            if(!NewBuff.IsInfinityDuration)
            {
                //5) seteamos el duration del buff
                buffDurationDictionary[finalbuff] = NewBuff.BuffDuration;

            }
            //6) le enviamos la referencia del scriptableObject al script.
            finalbuff.SetSO(NewBuff);
        }

        print("buffo aplicado correctamente " + NewBuff.BuffName);
    }


    //esta lista cachea los buffos expirados
     List<BuffBehavior> ExpiredBuffs = new List<BuffBehavior>();
  
    private void Update()
    {
        if(BuffDictionary.Count > 0)
        {
            foreach(BuffBehavior buffScript in BuffDictionary.Values)
            {
                buffScript.OnUpdate(Time.deltaTime);
                if (!buffDurationDictionary.ContainsKey(buffScript))
                {
                    //skip
                    continue;
                }

                // chequeamos si el buffo ya debería de haber expirado
                if ((buffDurationDictionary[buffScript]) <= 0)
                {
                    // cacheamos el buffo expirado. hacemos esto porque modificar
                    // la coleccion en un foreach == muerte
                   ExpiredBuffs.Add(buffScript); 
                }
                else
                {
                    buffDurationDictionary[buffScript] -= Time.deltaTime;
                }
            }
        }
        
        if(DeBuffDictionary.Count > 0)
        {
            foreach (BuffBehavior DebuffScript in DeBuffDictionary.Values)
            {
                DebuffScript.OnUpdate(Time.deltaTime);

                if (!buffDurationDictionary.ContainsKey(DebuffScript))
                {
                    //skip
                    continue;
                }

                // chequeamos si el Debuffo ya debería de haber expirado
                if (buffDurationDictionary[DebuffScript] <= 0)
                {
                    // cacheamos el buffo expirado. hacemos esto porque modificar
                    // la coleccion en un foreach == muerte
                    ExpiredBuffs.Add(DebuffScript);
                }
                else
                {
                    buffDurationDictionary[DebuffScript] -= Time.deltaTime;
                }
            }
        }

        //ahora vamos a eliminar todo buffo que este expirado
        for(int i = 0; i < ExpiredBuffs.Count; i++)
        {
            var Buff = ExpiredBuffs[i];

            if(Buff.GetSO().BuffType == BuffTypes.Buff)
            {
                ExpireBuff(Buff, BuffDictionary);
            }
            else
            {
                ExpireBuff(Buff, DeBuffDictionary);
            }

            //eliminamos la referencia del buff expirado en el diccionario de duraciones
            buffDurationDictionary.Remove(Buff);


        }

        if(ExpiredBuffs.Count > 0)
        {
            //limpiamos listado
            ExpiredBuffs.Clear();
        }
    }

    private void ExpireBuff(BuffBehavior BuffToExpire,Dictionary<string,BuffBehavior> dic)
    {
        // llamamos al OnExpire del buff;
        BuffToExpire.OnExpire();

        // removemos al buff del diccionario en cuestion
        dic.Remove(BuffToExpire.GetSO().BuffID);

        Destroy(BuffToExpire);
    }
}
