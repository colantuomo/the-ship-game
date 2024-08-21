using UnityEditor;
using UnityEngine;
using System.IO;

public class RemoveUnusedAssets : MonoBehaviour
{
    [MenuItem("Tools/Remove Unused Assets")]
    public static void RemoveAllUnusedAssets()
    {
        // Caminho da pasta de Assets
        string assetPath = "Assets/Epic Toon FX";

        // Obtenha todos os assets no projeto
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        foreach (string asset in allAssets)
        {
            // Ignore arquivos fora da pasta 'Assets/'
            if (!asset.StartsWith(assetPath)) continue;

            // Verifique se o asset é usado
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(asset);
            if (obj == null || !IsAssetUsedInScenes(asset))
            {
                Debug.Log("Removendo asset não utilizado: " + asset);
                AssetDatabase.DeleteAsset(asset);
            }
        }

        AssetDatabase.Refresh();
    }

    // Verifique se o asset está sendo referenciado em alguma cena
    private static bool IsAssetUsedInScenes(string asset)
    {
        string[] scenes = AssetDatabase.FindAssets("t:Scene");

        foreach (string scene in scenes)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(scene);
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);

            foreach (string dependency in dependencies)
            {
                if (dependency == asset)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
