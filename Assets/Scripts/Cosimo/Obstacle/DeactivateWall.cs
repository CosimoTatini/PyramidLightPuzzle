using System;
using UnityEngine;

public class DeactivateWall : MonoBehaviour
{
    [SerializeField] private bool _removeOnEternalTorchRemoved = false;
    [SerializeField] private GameObject _wallGameObject;

    private void OnEnable()
    {
        if (_removeOnEternalTorchRemoved)
            PlacementManager.OnEternalTorchRemoved += HandleTorchRemoved;
    }

    private void OnDisable()
    {
        if (_removeOnEternalTorchRemoved)
            PlacementManager.OnEternalTorchRemoved -= HandleTorchRemoved;
    }

    public void HandleTorchRemoved()
    {
        ToggleWall(false);
    }

    public void ToggleWall(bool toggle)
    {
        _wallGameObject.SetActive(toggle);
    }
}
