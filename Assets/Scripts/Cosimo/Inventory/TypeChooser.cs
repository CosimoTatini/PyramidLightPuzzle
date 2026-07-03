using UnityEngine;

public class TypeChooser : MonoBehaviour
{
    [SerializeField] private TorchType _type = TorchType.Normal;
    [SerializeField] private bool _isEternal = false;

    public TorchType Type => _type;

    public bool IsPrexistent { get;set; }

    public bool IsEternal
    {
        get => _isEternal;
        set => _isEternal = value;
    }
}
