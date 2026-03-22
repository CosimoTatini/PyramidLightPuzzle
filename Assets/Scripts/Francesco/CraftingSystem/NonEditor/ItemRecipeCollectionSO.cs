using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a collection of item recipes used for crafting or combination logic, it is intended to be turned into a dictionary 
/// during runtime for efficient lookup.
/// Can be used to also have multiple configuration of recipes, for example in one world an item is crafted with a specific recipe, while in another world it is crafted with another one.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/" + nameof(ItemRecipeCollectionSO))]
public class ItemRecipeCollectionSO : ScriptableObject
{
    [SerializeField] private List<ItemRecipeSO> _itemRecipes = new();
    public List<ItemRecipeSO> ItemRecipes => _itemRecipes;

    private void OnValidate()
    {
        _itemRecipes.RemoveAll(recipe => recipe == null);
    }

    public Dictionary<ItemSO, ItemRecipeSO[]> GetDictionaryOfRecipes(out List<ItemRecipeSO> recipesWithNullResults)
    {
        Dictionary<ItemSO, List<ItemRecipeSO>> tempDict = new();

        recipesWithNullResults = new();

        foreach (var recipe in _itemRecipes)
        {
            if (!recipe) continue;
            if (!recipe.ResultingSO)
            {
                recipesWithNullResults.Add(recipe);
                continue;
            }
            if (!tempDict.ContainsKey(recipe.ResultingSO))
            {
                tempDict[recipe.ResultingSO] = new List<ItemRecipeSO>();
            }

            if (tempDict[recipe.ResultingSO].Contains(recipe))
            {
                Debug.LogWarning($"The recipe {recipe.name} is already present in the dictionary for the item {recipe.ResultingSO.name}, skipping it to avoid duplicates.");
                continue;
            }
            tempDict[recipe.ResultingSO].Add(recipe);
        }

        return tempDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}