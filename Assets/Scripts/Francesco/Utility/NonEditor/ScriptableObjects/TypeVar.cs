using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(ScriptableObject) + "/" + nameof(TypeVar))]

public class TypeVar : ScriptableObject
{
    [SerializeField] private string _typeName;
    [SerializeField] private string _filter;
    [SerializeField] private bool _includeUnityAssemblies;
    [SerializeField] private string _assemblyPath;

    public Type Type
    {
        get => string.IsNullOrEmpty(_typeName) ? null : Type.GetType(_typeName);
        set => _typeName = value?.AssemblyQualifiedName;
    }
}
