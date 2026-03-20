using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // PATO: esto me permite manejar herramientas del editor de unity
using System.IO; // here be dragons(?) // me permite manejar directorios del sistema (momento borrar win 32)


public class VS_LevelGrowthTool : EditorWindow
{

    // variable del nombre del objeto nuevo
    string newAssetName = "New Level Growth Profile";

    // NOTA: camino objetivo para guardar el StatID, basicamente a donde lo vamos a meter 
    const string TARGET_PATH = "Assets/ScriptableObjects/Gameplay/LevelUpProfiles";

    [MenuItem("VisceralLimbo/Tools/levelUpProfileMaker")]

    public static void ShowWindow()
    {

        //esta funcion permite crear una ventana rectangular de Unity usando este script como su logica,eso es lo que entendi xd
        GetWindow<VS_LevelGrowthTool>("Level Up Profile Maker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Up Profile Maker - Visceral Limbo", EditorStyles.boldLabel);

        // campo de texto para nombrar el asset
        newAssetName = EditorGUILayout.TextField("Nombre del Profile:", newAssetName);

        // un pequeño espacio chulo
        EditorGUILayout.Space();

        if (GUILayout.Button("Crear Level Up Profile"))
        {
            // catch! tratas de poner un nombre vacio
            if (string.IsNullOrEmpty(newAssetName))
            {
                Debug.LogError("<Color = color.orange> [Visceral Tool Error]: Falta nombre en asset </Color>");
                return;
            }
            CreateMyAsset(newAssetName);
        }
    }

    private void CreateMyAsset(string name)
    {
        // 1. chequeamos que no haya carpeta destino
        if (!AssetDatabase.IsValidFolder(TARGET_PATH))
        {
            // Si falla, intentamos de refrescar y nos morimos en el medio
            Debug.LogWarning("La carpeta parece no existir para Unity. Intentando forzar refresh...");
            AssetDatabase.Refresh();
        }

        // 2. Crear la instancia en memoria
        LevelUpProfileSO asset = ScriptableObject.CreateInstance<LevelUpProfileSO>();

        // 3. Crear ruta completa
        string finalPath = TARGET_PATH + "/" + name + ".asset";

        // 4. Asegurar nombre único
        finalPath = AssetDatabase.GenerateUniqueAssetPath(finalPath);

        // 5. Guardar en disco 
        AssetDatabase.CreateAsset(asset, finalPath);
        AssetDatabase.SaveAssets();

        // 6. seleccionamos el objeto en el editor, porque soy muy -BONITA- 
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log("Asset creado en: " + finalPath);
    }






}
