using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;

public class PowderColorChooser : MonoBehaviour
{
    [SerializeField]private PowderColor _color= PowderColor.Red;

    public PowderColor Color => _color;
}
