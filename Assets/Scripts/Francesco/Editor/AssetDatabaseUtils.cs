using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AD = UnityEditor.AssetDatabase;
using System.Linq;

/// <summary>
/// AssetDatabase extension methods
/// </summary>
public static class AssetDatabaseUtils
{
    // Old GetAssetsByType
    //public static T[] GetAssetsByType<T>() where T : Object
    //{
    //    string[] GUIDs = GetGUIDs($"{typeof(T).Name}");

    //    if (GUIDs == null) return null;

    //    T[] assets = new T[GUIDs.Length];

    //    for (int i = 0; i < GUIDs.Length; i++)
    //    {
    //        T asset = LoadAssetByGUID<T>(GUIDs[i]);
    //        assets[i] = asset;
    //    }

    //    return assets;
    //}

    /// <summary>
    /// Retrives the assets of the passed type
    /// </summary>
    /// <typeparam name="T">The type of the assets to retrieve</typeparam>
    /// <returns></returns>
    public static T[] GetAssetsByType<T>() where T : Object
    {
        return GetAssets($"t:{typeof(T).Name}")
            ?.Cast<T>()
            ?.ToArray();
    }

    /// <summary>
    /// Retrieves and load the assets specified in the query
    /// </summary>
    /// <param name="query">The query you would type in Project tab</param>
    /// <returns></returns>
    public static Object[] GetAssets(string query)
    {
        string[] GUIDs = GetGUIDs(query);

        if (GUIDs == null) return null;

        Object[] assets = new Object[GUIDs.Length];

        for (int i = 0; i < GUIDs.Length; i++)
        {
            Object asset = LoadAssetByGUID<Object>(GUIDs[i]);
            assets[i] = asset;
        }

        return assets;
    }

    /// <summary>
    /// Retrieves all of the Prefabs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static T[] GetPrefabsWithComponent<T>(string name = "") where T : Component
    {
        Object[] assets = GetAssets($"{name} t:prefab");
        if (assets == null) return null;
        T[] gameObjects = assets
            .OfType<GameObject>()
            .Select(obj => obj.GetComponent(typeof(T))).Where(comp => comp != null).Cast<T>().ToArray();
        return (gameObjects.Length == 0) ? null : gameObjects;
    }

    /// <summary>
    /// Retrieves the GUIDs of the assets corresponding to the query
    /// </summary>
    /// <param name="query">The query you would type in Project tab</param>
    /// <returns>The GUIDs of the assets matching the query</returns>
    public static string[] GetGUIDs(string query)
    {
        string[] GUIDs = AD.FindAssets(query);
        if (GUIDs.Length == 0) return null;
        return GUIDs;
    }

    /// <summary>
    /// Loads an asset with its guid
    /// </summary>
    /// <typeparam name="T">The object's type</typeparam>
    /// <param name="GUID">The asset's GUID</param>
    /// <returns>The loaded asset or null if GUID is not valid</returns>
    public static T LoadAssetByGUID<T>(string GUID) where T : Object
    {
        return (T)AD.LoadAssetAtPath(AD.GUIDToAssetPath(GUID), typeof(T));
    }

    public static void SaveAndRefresh()
    {
        AD.SaveAssets();
        AD.Refresh();
    }
}
