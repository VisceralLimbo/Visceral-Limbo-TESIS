using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInterfaces 
{
    
}

public interface I_ItemPassiveItem
{

}

/// <summary>
/// Interfaz usada para crear items activos, osea cuyos efectos tomen efecto con el tiempo
/// basicamente un update.
/// </summary>
public interface I_ItemActiveItem
{
    /// <summary>
    /// Funcion de update Loop, poner aquí la logica de item activo
    /// </summary>
    /// <param name="Manager">referencia al inventory manager</param>
    /// <param name="StatManager">referencia al stat manager</param>
    public abstract void UpdateActiveItem(InventoryManager Manager,StatsManager StatManager);
}

public interface I_ItemStatModifier
{

}

public interface I_ItemTriggeredItem
{

}

public interface I_ItemOnKill
{

}

