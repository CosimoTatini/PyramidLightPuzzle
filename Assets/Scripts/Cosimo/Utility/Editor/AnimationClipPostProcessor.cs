using UnityEditor;
using UnityEngine;

public class AnimationClipPostProcessor : AssetModificationProcessor
{
    // Questo metodo viene chiamato da Unity ogni volta che viene creato un nuovo asset
    static void OnWillCreateAsset(string assetName)
    {
        // Controlliamo se l'asset che sta per essere creato è una clip di animazione (.anim)
        if (assetName.EndsWith(".anim.meta"))
        {
            // Usiamo un leggero ritardo per permettere a Unity di finire la creazione del file
            EditorApplication.delayCall += () => SetFrames(assetName.Replace(".meta", ""));
        }
    }

    static void SetFrames(string path)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip != null)
        {
            clip.frameRate = 24; // Impostiamo il nostro standard
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Editor] Default impostato: {clip.name} ora gira a 24 FPS.");
        }
    }
}
