using UnityEngine;

public abstract class ItemInteraction : PlayerPriorityInteractable
{
    [SerializeField] protected ItemPlacement _itemPlacement;
    public ItemPlacement ItemPlacement => _itemPlacement;

    public override void Interact()
    {
    }
}
