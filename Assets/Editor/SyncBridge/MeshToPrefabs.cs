using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

public class MeshToPrefabs : MonoBehaviour
{


    [MenuItem("SyncBridge/Sync It")]
    static void Main()
    {
        
    }



    static void Settings()
    {
        string path = "Assets/Editor/SyncBridge";
        string settingsPath = path + "/Settings";

        // Create Settings folder
        if (AssetDatabase.IsValidFolder(path + "/Settings"))
        {
            return;
        }
        else
        {
            AssetDatabase.CreateFolder(path, "Settings");

            Dictionary<string, int> syncFolders = new Dictionary<string, int>()
            {
                {"Ass", 1},
            };

            string jsonString = JsonUtility.ToJson(syncFolders);
            File.WriteAllText(settingsPath + "/FoldersSyncSettings.json", jsonString);

        }





    }

    [MenuItem("Assets/Sync Folder")]
    static void SyncFolder()
    {
        // Get path then you make asign
        string folderPath = GetPath();

        // Create /meshes and /prefabs and /materials folders if not exist
        if (!AssetDatabase.IsValidFolder(folderPath + "/meshes"))
        {
            AssetDatabase.CreateFolder(folderPath, "meshes");
        }
        
        if (!AssetDatabase.IsValidFolder(folderPath + "/prefabs"))
        {
            AssetDatabase.CreateFolder(folderPath, "prefabs");
        }

        if (!AssetDatabase.IsValidFolder(folderPath + "/materials"))
        {
            AssetDatabase.CreateFolder(folderPath, "materials");
        }

        string meshFolderPath = folderPath + "/meshes";
        string prefabFolderPath = folderPath + "/prefabs";

        // Get all mesh assets in the mesh folder
        string[] meshAssetPaths = AssetDatabase.FindAssets("t:Mesh", new[] { meshFolderPath });

        foreach (string meshAssetPath in meshAssetPaths)
        {
            // Load the mesh asset
            string path = AssetDatabase.GUIDToAssetPath(meshAssetPath);



            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(meshAssetPath));

            string assetName = asset.name;

            // Check if the mesh name includes the string "PREFABS"
            if (assetName.Contains("_PREFAB"))
            {
                // Create a new game object and add the mesh as a component
                GameObject gameObject = new GameObject(assetName.Replace("_PREFAB", ""));
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = asset.GetComponent<MeshFilter>().sharedMesh;


                // Extract and assign material

                Material[] materials = asset.GetComponent<MeshRenderer>().sharedMaterials;
                Material[] newMaterials = new Material[materials.Length];
                 
                for (int i = 0; i < materials.Length; i++)
                {
                    string matPath = folderPath + "/materials/" + materials[i].name + ".mat";
                    Material temp = Instantiate(materials[i]);
                    AssetDatabase.CreateAsset(temp, matPath);
                    newMaterials[i] = AssetDatabase.LoadAssetAtPath<Material>(matPath);

                }
                 
                meshRenderer.sharedMaterials = newMaterials;

                // Create a prefab asset for the game object
                string prefabPath = prefabFolderPath + "/" + assetName.Replace("_PREFAB", "") + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);

                // Destroy the temporary game object
                DestroyImmediate(gameObject);
            }
        }

    }

    static void ExtractMaterials(string assetPath, string folderPath)
    {
        //Try to extract materials into a subfolder
        var assetsToReload = new HashSet<string>();
        var materials = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(x => x.GetType() == typeof(Material)).ToArray();
        Debug.Log(assetPath + " has " + materials.Length + " materials");
        foreach (var material in materials)
        {
            var newAssetPath = folderPath + "/materials/" + material.name + ".mat";
            var error = AssetDatabase.ExtractAsset(material, newAssetPath);
            if (String.IsNullOrEmpty(error))
            {
                assetsToReload.Add(assetPath);
            }
        }

        foreach (var path in assetsToReload)
        {
            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

    static string GetPath()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        return path;

    }

    
}
