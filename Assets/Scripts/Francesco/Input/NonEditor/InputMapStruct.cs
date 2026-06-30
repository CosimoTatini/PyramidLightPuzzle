using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct InputMapStruct
{
    public string Guid;
    public string Name;
    public List<InputActionEntry> InputActionEntries;

    public InputActionEntry GetInputActionStruct(string guid)
    {
        if (InputActionEntries == null || InputActionEntries.Count == 0) return null;
        var result = InputActionEntries.Where(action => action.Guid == guid);
        if(result.Count() == 0) return null;
        return result.ElementAt(0);
    }
}