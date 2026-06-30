using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/" + nameof(SpriteAssetsInputList))]
public class SpriteAssetsInputList : ScriptableObject
{
    [SerializeField] private List<SpriteAssetInputScheme> _spriteAssetInputSchemes;
    public List<SpriteAssetInputScheme> SpriteAssetInputSchemes => _spriteAssetInputSchemes;
}