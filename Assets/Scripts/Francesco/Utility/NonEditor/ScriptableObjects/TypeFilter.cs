using System;
using UnityEngine;

[Serializable]
public struct TypeFilter
{
    [SerializeField] private TypeVar _typeVar;
    [SerializeField] private bool _strictMode;

    public TypeVar TypeVar => _typeVar;
    public bool StrictMode => _strictMode;
}
