using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ColoredDustRecipe
{
    public List<ColoredDustQuantity> Ingredients;
}

[Serializable]
public struct ColoredDustQuantity
{
    public ColoredDust Dust;
    [Min(1)]
    public int Quantity;
}

[CreateAssetMenu(menuName = "LightSystem/" + nameof(ColoredDust))]
public class ColoredDust : ScriptableObject
{
    [Tooltip("Base sprite used with color if no custom sprite is assigned")]
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private Sprite _baseSprite;
    [SerializeField] private Sprite _personalizedSprite;
    [SerializeField] private ColoredDustRecipe _recipe;

    public Color Color => _color;
    public Sprite BaseSprite => _baseSprite;
    public Sprite PersonalizedSprite => _personalizedSprite;
    public ColoredDustRecipe Recipe => _recipe;
}
