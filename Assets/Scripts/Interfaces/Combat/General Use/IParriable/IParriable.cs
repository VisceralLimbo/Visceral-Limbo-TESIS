using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IParriable
{
    void parried(DamageScore? DMScore,Vector3 Direction = default);
}
