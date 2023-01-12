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

    static string folderPath;

    static string sourceFolder;
    static string meshesFolder;
    static string prefabsFolder;
    static string materialsFolder;

    static string sceneName;
    static string sceneMeshPath;


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


        if (!SetUpFolders())
        {
            return;
        }


        if (!GenerateScenePrefabs())
        {
            return;
        }

    }


    static bool SetUpFolders()
    {


        // Get path then you make asign
        folderPath = GetPath();
        // Set folders
        meshesFolder = folderPath + "/meshes/";
        prefabsFolder = folderPath + "/prefabs/";
        materialsFolder = folderPath + "/materials/";

        sourceFolder = folderPath + "/_source/";

        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
            Debug.LogError("Nothing to sync: make sure you have blend file into _source folder!");
            return false;
        }

        sceneName = folderPath.Substring(folderPath.LastIndexOf("/"));
        sceneMeshPath = meshesFolder + sourceFolder + sceneName + ".blend";

        // Create folders if not exist
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

        return true;
    }

    static bool GenerateScenePrefabs()
    {
        // Get all mesh assets in the mesh folder
        string[] guid = AssetDatabase.FindAssets("t:Mesh", new[] { sourceFolder });

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid[0]));

        Mesh test = asset.GetComponent<Mesh>();

        int prefabCount = 0;

        //foreach (string meshGUIDS in meshesGUIDSFromSource)
        //{
        //
        //    
        //
        //    
        //    string assetName = asset.name;
        //
        //    Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        //
        //    // Check if the mesh name includes the "."
        //    if (!assetName.Contains("."))
        //    {
        //        // Generate Prefab
        //
        //        prefabCount += 1;
        //
        //        // Create a new game object and add the mesh as a component
        //        GameObject gameObject = new GameObject(assetName);
        //        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        //        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        //        meshFilter.sharedMesh = asset.GetComponent<MeshFilter>().sharedMesh;
        //
        //
        //        // Extract and assign material
        //
        //        Material[] materials = asset.GetComponent<MeshRenderer>().sharedMaterials;
        //        Material[] newMaterials = new Material[materials.Length];
        //
        //        for (int i = 0; i < materials.Length; i++)
        //        {
        //            string matPath = materialsFolder + materials[i].name + ".mat";
        //            Material temp = Instantiate(materials[i]);
        //            AssetDatabase.CreateAsset(temp, matPath);
        //            newMaterials[i] = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        //
        //        }
        //
        //        meshRenderer.sharedMaterials = newMaterials;
        //
        //        // Create a prefab asset for the game object
        //        string prefabPath = prefabsFolder + assetName + ".prefab";
        //        PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
        //
        //        // Destroy the temporary game object
        //        DestroyImmediate(gameObject);
        //
        //    } else
        //    {
        //        // Generate scene mesh
        //
        //        // Delete all after dot in name
        //        string prefabName = asset.transform.name;
        //        int index = prefabName.IndexOf(".");
        //        prefabName = prefabName.Substring(0, index);
        //
        //
        //        // Get the prefab asset with the same name as the child transform
        //        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsFolder + prefabName + ".prefab");
        //
        //        // Instantiate the prefab in the scene
        //        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        //
        //        // Set the position and rotation of the instance to match the child transform
        //        instance.transform.position = asset.transform.position;
        //        instance.transform.rotation = asset.transform.rotation;
        //
        //
        //        // Save the scene
        //        EditorSceneManager.SaveScene(newScene, folderPath + sceneName + ".unity");
        //    }
        //}

        if(prefabCount == 0)
        {
            Debug.LogError("Source blend file has no any prefabs: make sure your prefabs ain't contains '.' symbol.");
            return false;
        }

        return true;
    }


    static bool GenerateSceneByMesh()
    {
        // Get scene fbx
        GameObject sceneMesh = AssetDatabase.LoadAssetAtPath<GameObject>(sceneMeshPath);
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
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsFolder + prefabName + ".prefab");

            // Instantiate the prefab in the scene
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            // Set the position and rotation of the instance to match the child transform
            instance.transform.position = childTransform.position;
            instance.transform.rotation = childTransform.rotation;
        }

        return true;
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
