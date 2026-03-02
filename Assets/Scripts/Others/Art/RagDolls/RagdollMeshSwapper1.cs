using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollMeshSwapper1 : MonoBehaviour
{
    // Dejo tus variables extra por si las usas luego
    [SerializeField] GameObject[] MeshSwapPrefabs;
    [SerializeField] GameObject[] MeshToSwap;

    [SerializeField] SkinnedMeshRenderer IntactMeshSkin;
    [SerializeField] SkinnedMeshRenderer[] GoredMeshSkin;

    private void Awake()
    {
        // 1. Guardamos todos los huesos del modelo Intacto en un diccionario (Llave: Nombre, Valor: Transform)
        Dictionary<string, Transform> intactBoneMap = new Dictionary<string, Transform>();

        foreach (Transform bone in IntactMeshSkin.bones)
        {
            if (bone != null && !intactBoneMap.ContainsKey(bone.name))
            {
                intactBoneMap.Add(bone.name, bone);
            }
        }

        // 2. Revisamos cada pedazo del modelo Gored
        foreach (SkinnedMeshRenderer SKMR in GoredMeshSkin)
        {
            // Tomamos los huesos que el Gored trajo de Maya (aunque no estén atados a un Animator)
            Transform[] oldBones = SKMR.bones;

            // Creamos una lista nueva del mismo tamaño exacto que necesita este pedazo
            Transform[] newBones = new Transform[oldBones.Length];

            // 3. Emparejamos uno por uno buscando el nombre en el diccionario del Intacto
            for (int i = 0; i < oldBones.Length; i++)
            {
                if (oldBones[i] != null && intactBoneMap.ContainsKey(oldBones[i].name))
                {
                    // ¡Coincidencia encontrada! Le pasamos el hueso vivo del Intacto
                    newBones[i] = intactBoneMap[oldBones[i].name];
                }
                else
                {
                    Debug.LogWarning("No se encontró el hueso " + (oldBones[i] != null ? oldBones[i].name : "NULL") + " en el modelo Intacto.");
                }
            }

            // Aplicamos la lista corregida y el Root Bone
            SKMR.bones = newBones;
            SKMR.rootBone = IntactMeshSkin.rootBone;
        }
    }
}

