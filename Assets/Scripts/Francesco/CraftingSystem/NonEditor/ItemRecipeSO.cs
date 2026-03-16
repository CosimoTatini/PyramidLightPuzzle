using System;
using UnityEngine;

public class ItemRecipeSO : ScriptableObject
{
    [SerializeField] private ItemSO _resultingSO;
    [SerializeField] private ItemRecipeQuantity[] _requiredItems;

    public ItemSO ResultingSO { get { return _resultingSO; } set { _resultingSO = value; } }
    public ItemRecipeQuantity[] RequiredItems { get { return _requiredItems; } set { _requiredItems = value; } }
}

[Serializable]
public struct ItemRecipeQuantity
{
    public ItemSO ItemSO;
    public int Quantity;
}