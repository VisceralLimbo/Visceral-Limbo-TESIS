using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BloodEchoesManager
{

    private static float _BloodEchoes;
    // variable para guardarme los echoes viejos
    private static float _PreviousBloodEchoes;

    public static float BloodEchoes => _BloodEchoes;
    public static float PreviousBloodEchoes => _PreviousBloodEchoes;

    /// <summary>
    /// evento que se llama cada q cambian los blood echoes
    /// </summary>
    /// 

    // para que muestre el total y el cambio (sea suma o resta)
    public static event Action<float, float> OnBloodEchoesChanged;

    /// <summary>
    /// añadir mas ecos de sangre al jugador
    /// </summary>
    /// <param name="amount">cantidad de ecos de sangre</param>
    public static void AddBloodEchoes(float amount)
    {
        _PreviousBloodEchoes = _BloodEchoes; // guardo los echoes viejos antes de cambiarlo
        _BloodEchoes += amount;
        // mando el total y lo q se suma
        OnBloodEchoesChanged?.Invoke(_BloodEchoes, amount);
    }

    /// <summary>
    /// comprar con ecos de sangre, esta funcion es una compra directa
    /// no chequea nada
    /// </summary>
    /// <param name="cost"> restar ecos de sangre </param>
    public static void PurchaseBloodEchoes(float cost)
    {
        _PreviousBloodEchoes = _BloodEchoes; // guardo los echoes viejos antes de cambiarlo
        _BloodEchoes -= cost;
        // envio total y cantidad que se resta
        OnBloodEchoesChanged?.Invoke(_BloodEchoes, -cost);
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
