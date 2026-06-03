using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct InputMapStruct
{
    public string Guid;
    public List<InputActionStruct> InputActionStructs;

    public InputActionStruct? GetInputActionStruct(string guid)
    {
        if (InputActionStructs == null || InputActionStructs.Count == 0) return null;
        var result = InputActionStructs.Where(action => action.Guid == guid);
        if(result.Count() == 0) return null;
        return result.ElementAt(0);
    }
}