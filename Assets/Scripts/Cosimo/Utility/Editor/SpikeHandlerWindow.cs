
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpikeHandlerWindow : EditorWindow
{
    [SerializeField] private List<SpikeHandler> _spikes = new List<SpikeHandler>();

    private bool _hasAppliedSequence = false;

    [MenuItem("Tools/SpikeHandler")]
    private static void ShowWindow()
    {
        GetWindow<SpikeHandlerWindow>("SpikeHandler");
    }

    private void OnEnable()
    {
        _hasAppliedSequence = false;
    }

    private void OnGUI()
    {
        GUILayout.Label("Spike Sequence Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. Gestione della lista nell'interfaccia dell'Editor
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("_spikes");
        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();

        EditorGUILayout.Space();

        // 2. Controllo dello stato del bottone
        // Se _hasAppliedSequence è true, il bottone viene disattivato (grigio)
        EditorGUI.BeginDisabledGroup(_hasAppliedSequence);

        if (GUILayout.Button("Calcola Sequenza Delay (+1s)", GUILayout.Height(30)))
        {
            ApplyDelaySequence();
        }

        EditorGUI.EndDisabledGroup();

        if (_hasAppliedSequence)
        {
            EditorGUILayout.HelpBox("Sequenza applicata! Il bottone si riattiverà riaprendo la finestra.", MessageType.Info);
        }
    }

    private void ApplyDelaySequence()
    {
        if (_spikes == null || _spikes.Count == 0)
        {
            Debug.LogWarning("La lista delle spine è vuota!");
            return;
        }

        // Algoritmo lineare: imposta il delay progressivo (+1 rispetto al precedente)
        for (int i = 0; i < _spikes.Count; i++)
        {
            if (_spikes[i] != null)
            {
                // Registra l'operazione per consentire l'Undo (Ctrl+Z) in Unity, un tocco da Senior!
                Undo.RecordObject(_spikes[i], "Auto Assign Spike Delay");

                // Assegna il valore matematico (0, 1, 2, 3...)
                _spikes[i].Delay = i;

                // Forza Unity a salvare la modifica sull'oggetto in scena o nel prefab
                EditorUtility.SetDirty(_spikes[i]);
            }
        }

        // Blocca il bottone fino alla riapertura della finestra
        _hasAppliedSequence = true;
        Debug.Log("I delay delle spine sono stati aggiornati con successo!");
    }
}
