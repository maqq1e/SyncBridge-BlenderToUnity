using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

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

        string meshesFolder = folderPath + "/meshes";
        string prefabsFolder = folderPath + "/prefabs";
        string materialsFolder = folderPath + "/materials";

        string sceneName = folderPath.Substring(folderPath.LastIndexOf("/"));
        string sceneMeshPath = meshesFolder + sceneName + ".fbx";

        // Create /meshes and /prefabs and /materials folders if not exist
        if (!AssetDatabase.IsValidFolder(meshesFolder))
        {
            AssetDatabase.CreateFolder(folderPath, "meshes");
        }
        
        if (!AssetDatabase.IsValidFolder(prefabsFolder))
        {
            AssetDatabase.CreateFolder(folderPath, "prefabs");
        }

        if (!AssetDatabase.IsValidFolder(materialsFolder))
        {
            AssetDatabase.CreateFolder(folderPath, "materials");
        }

        string meshFolderPath = folderPath + "/meshes";
        string prefabFolderPath = folderPath + "/prefabs";

        // Get all mesh assets in the mesh folder
        string[] meshAssetPaths = AssetDatabase.FindAssets("t:Mesh", new[] { meshFolderPath });

        foreach (string meshAssetPath in meshAssetPaths)
        {

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

        GenerateSceneByMesh(sceneMeshPath, prefabsFolder, folderPath + sceneName + ".unity");

    }

    static void GenerateSceneByMesh(string meshPath, string prefabsPath, string scenePath)
    {
        // Get scene fbx
        GameObject sceneMesh = AssetDatabase.LoadAssetAtPath<GameObject>(meshPath);
        Transform meshTransform = sceneMesh.transform;

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        for (int i = 0; i < meshTransform.childCount; i++)
        {
            // Get the child transform
            Transform childTransform = meshTransform.GetChild(i);

            // Delete all after dot in name
            string prefabName = childTransform.name;
            int index = prefabName.IndexOf(".");
            prefabName = prefabName.Substring(0, index);
                

            // Get the prefab asset with the same name as the child transform
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "/" + prefabName + ".prefab");

            // Instantiate the prefab in the scene
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            // Set the position and rotation of the instance to match the child transform
            instance.transform.position = childTransform.position;
            instance.transform.rotation = childTransform.rotation;
        }

        // Save the scene
        EditorSceneManager.SaveScene(newScene, scenePath);
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
