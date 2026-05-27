using UnityEngine;

public class TypeChooser : MonoBehaviour
{
    [SerializeField] private TorchType _type = TorchType.Normal;

    public TorchType Type => _type;
}
