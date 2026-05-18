using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]

public class InventoryManager : MonoBehaviour
{
    public List<Item> Items= new List<Item>();
    private int _selectedIndex = 0;

    public Item GetSelectedItem()=> Items[_selectedIndex];

    public void ChangeSelection()
    {
        _selectedIndex= (_selectedIndex+1) % Items.Count;
    }

    public void AddItem(string name,int amount)
    {
        var item = Items.Find(i => i.name == name);
        if(item!=null)
        {
            item.Quantity += amount;
        }
    }
    public void RemoveItem(string name, int amount)
    {
        var item= Items.Find(i => i.name == name);
        if(item!=null)
        {
            item.Quantity= Mathf.Max(0, item.Quantity -amount);
        }
    }

}
