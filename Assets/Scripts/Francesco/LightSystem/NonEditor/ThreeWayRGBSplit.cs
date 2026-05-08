using UnityEngine;

public class ThreeWayRGBSplit : MonoBehaviour
{
    [SerializeField] private LightSensor _lightSensor;
    [Header("Light Emitters")]
    [SerializeField] private LightEmitter _redEmitter;
    [SerializeField] private LightEmitter _greenEmitter;
    [SerializeField] private LightEmitter _blueEmitter;
}
