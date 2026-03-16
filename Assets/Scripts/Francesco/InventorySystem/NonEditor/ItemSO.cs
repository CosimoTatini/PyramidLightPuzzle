using UnityEngine;

public class ItemSO : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _maxStackSize = 1;

    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int MaxStackSize => _maxStackSize;
}
