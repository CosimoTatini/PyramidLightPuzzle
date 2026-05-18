using UnityEngine;

public class ColoredDustEditorData : ScriptableObject
{
    [SerializeField] private Color _baseDustColor = Color.white;
    [SerializeField] private Sprite _baseSprite;
    [SerializeField] private string _assetPathToCreateNewData = "Assets/ScriptableObjects/ColoredDusts";
    public Color BaseDustColor => _baseDustColor;
    public Sprite BaseSprite => _baseSprite;
    public string AssetPathToCreateNewData => _assetPathToCreateNewData;
}
