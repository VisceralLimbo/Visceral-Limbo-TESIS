using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpus_SlamEffect : MonoBehaviour
{
    public Material floorMaterial;
    public float radius = 3.0f;
    public float strength = 1.5f;

    public void OnSlamHit()
    {
        floorMaterial.SetVector("_CollisionPos", transform.position);
        floorMaterial.SetFloat("_Radius", radius);
        floorMaterial.SetFloat("_Strength", strength);
        Debug.Log("Se spawnea el shader!");
    }
}
