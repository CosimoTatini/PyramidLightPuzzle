using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlacementSetter : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    private void Start()
    {
       if(PlacementManager.Instance != null)
        {
            PlacementManager.Instance.InitializeTilemap(tilemap);
            Debug.Log($"[PlacementSetter] Tilemap inviata con successo al PlacementManager! ");
        }
       else
        {
            Debug.LogWarning("[PlacementSetter] Attenzione: PlacementManager.Instance non trovato! La scena dei manager è stata caricata? ");
        }

    }
}
