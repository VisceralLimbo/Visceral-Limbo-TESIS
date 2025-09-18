using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BloodEchoesManager
{

    private static float _BloodEchoes;

    public static float BloodEchoes => _BloodEchoes;

    /// <summary>
    /// evento que se llama cada q cambian los blood echoes
    /// </summary>

    public static event Action<float> OnBloodEchoesChanged;

    /// <summary>
    /// añadir mas ecos de sangre al jugador
    /// </summary>
    /// <param name="amount">cantidad de ecos de sangre</param>
    public static void AddBloodEchoes(float amount)
    {
        _BloodEchoes += amount;
        OnBloodEchoesChanged?.Invoke(_BloodEchoes); // llamo al evento para q se actualice
    }

    /// <summary>
    /// comprar con ecos de sangre, esta funcion es una compra directa
    /// no chequea nada
    /// </summary>
    /// <param name="cost"> restar ecos de sangre </param>
    public static void PurchaseBloodEchoes(float cost)
    {
        //se estaba llamando a todos los puntos en lugar de al coste
        _BloodEchoes -= cost;
        OnBloodEchoesChanged?.Invoke(_BloodEchoes); // llamo al evento para q se actualice
    }

    /// <summary>
    /// funcion para chequear si el jugador puede comprar algo.
    /// y si puede, retorna true y reduce blood echoes
    /// </summary>
    /// <param name="Cost">el costo de la compra</param>
    /// <returns></returns>
    public static bool CanPurchase(float Cost)
    {
        if (_BloodEchoes >= Cost)
        {
            PurchaseBloodEchoes(Cost);
            return true;

        }
        else
        {
            return false;
        }
    }
}
