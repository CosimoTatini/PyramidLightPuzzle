using System;
using UnityEngine;
using TMPro;

[Serializable]
public class SpriteAssetInputScheme
{
    [SerializeField] private string _schemeName;
    [SerializeField] private TMP_SpriteAsset _spriteAsset;

    public string SchemeName => _schemeName;
    public TMP_SpriteAsset SpriteAsset => _spriteAsset;
}