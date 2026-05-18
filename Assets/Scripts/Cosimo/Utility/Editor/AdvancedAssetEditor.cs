using System;
using UnityEditor;
using UnityEngine;

public class AdvancedAssetEditor : EditorWindow
{
    [MenuItem("Tools/Advanced Asset Editor")]
    
    public static void GetWindow()
    {
        GetWindow<AdvancedAssetEditor>("Asset Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configurazione Globale Editor", EditorStyles.boldLabel);

        // Pulsante per cambiare la visualizzazione della Timeline
        if (GUILayout.Button("Imposta Timeline su FRAMES"))
        {
            SetAnimationWindowToFrames();
        }
    }

    private void SetAnimationWindowToFrames()
    {
        // 1. Cambiamo la preferenza globale
        EditorPrefs.SetBool("AnimEditor.ShowFrame", true);

        // 2. Cerchiamo la finestra Animation specifica usando il suo tipo interno
        System.Type animWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");
        var window = EditorWindow.GetWindow(animWindowType);

        if (window != null)
        {
            // Forziamo il focus e il ridisegno della finestra
            window.Focus();
            window.Repaint();
        }

        // 3. Notifica globale di cambio impostazioni
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

        Debug.Log("[Asset Editor] Visualizzazione impostata su FRAMES. Se non cambia, prova a cliccare sulla timeline dell'Animation Window.");
    }
}
