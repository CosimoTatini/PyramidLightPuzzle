using System;
using UnityEngine;

public class ThreeWayRGBSPlitterDisableInteractionWhenRotating : MonoBehaviour
{
    [SerializeField] private ThreeWayRGBSplit _threeWayRGBSplit;
    [SerializeField] private InputConfigSO _disableRotationConfig;

    private void DisableRotationInteraction()
    {
        InputConfigManager.RegisterConfig(_disableRotationConfig);
    }

    private void EnableRotationInteraction()
    {
        InputConfigManager.UnregisterConfig(_disableRotationConfig);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player))
        {
            _threeWayRGBSplit.OnRotationStarted -= DisableRotationInteraction;
            _threeWayRGBSplit.OnRotationStarted += DisableRotationInteraction;
            _threeWayRGBSplit.OnRotationCompleted -= EnableRotationInteraction;
            _threeWayRGBSplit.OnRotationCompleted += EnableRotationInteraction;

            if (_threeWayRGBSplit.IsRotating)
            {
                DisableRotationInteraction();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player))
        {
            _threeWayRGBSplit.OnRotationStarted -= DisableRotationInteraction;
            _threeWayRGBSplit.OnRotationCompleted -= EnableRotationInteraction;
            EnableRotationInteraction();
        }
    }
}
