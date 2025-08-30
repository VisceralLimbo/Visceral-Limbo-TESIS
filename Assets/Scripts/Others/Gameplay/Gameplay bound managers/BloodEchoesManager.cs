using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BloodEchoesManager
{

    private static float _BloodEchoes;

    /// <summary>
    /// La cantidad actual de ecos de sangre
    /// </summary>
    public static float BloodEchoes { get { return _BloodEchoes; } }

    /// <summary>
    /// añadir mas ecos de sangre al jugador
    /// </summary>
    /// <param name="bloodEchoes">cantidad de ecos de sangre</param>
    public static void AddBloodEchoes(float bloodEchoes)
    {
        _BloodEchoes += bloodEchoes;
    }

    /// <summary>
    /// comprar con ecos de sangre, esta funcion es una compra directa
    /// no chequea nada
    /// </summary>
    /// <param name="Cost"> restar ecos de sangre </param>
    public static void PurchaseBloodEchoes(float Cost)
    {
        _BloodEchoes -= BloodEchoes;
    }

    /// <summary>
    /// funcion para chequear si el jugador puede comprar algo.
    /// y si puede, retorna true y reduce blood echoes
    /// </summary>
    /// <param name="Cost">el costo de la compra</param>
    /// <returns></returns>
    public static bool CanPurchase(float Cost)
    {
        if (_BloodEchoes - Cost > 0)
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
