using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NormalTorch : MonoBehaviour
{
    [SerializeField] private bool _isEternal;
    [SerializeField] private float _torchDuration = 30f;
    public bool IsEternal => _isEternal;

    void Start()
    {
        // eternal torches are the ones placed before loading the scene and don't count towards using torches
        if (!_isEternal)
        {
            StartCoroutine(TorchLifetimeCoroutine());
            InventoryManager.Instance.UseTorch();
        }
    }

    public IEnumerator TorchLifetimeCoroutine()
    {
        yield return new WaitForSeconds(_torchDuration);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (gameObject.scene.isLoaded)
        {
            InventoryManager.Instance.ReturnTorch(TorchType.Normal);
            if (_isEternal)
            {
                _isEternal = false;
                PlacementManager.InvokeEternalTorchRemoved();
            }
        }
        else
        {
            if (!_isEternal)
            {
                InventoryManager.Instance.ReturnTorch(TorchType.Normal);
            }
        }
    }
}