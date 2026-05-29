using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBundle
{
    private List<IInputActionCollection2> _inputActionCollections = new();

    public InputAction FindAction(Guid guid)
    {
        foreach (var inputCollection in _inputActionCollections)
        {
            InputAction action = inputCollection.FindAction(guid.ToString());
            if(action != null)
            {
                return action;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the instance of the input system of type T contained in the bundle. If there is no instance of type T, it creates it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The instance of type T of the corresponding InputSystemInstance</returns>
    // we make sure the passed T is a class, implements IInputActionCollection2 and has a parameterless constructor, so we can create an instance of it if it doesn't exist in the bundle
    public T GetInputSystemInstance<T>() where T : class, IInputActionCollection2, new()
    {
        foreach (var inputActionCollection in _inputActionCollections)
        {
            if (inputActionCollection is T t)
            {
                return t;
            }
        }

        T newInstance = new T();
        Debug.Log(newInstance);
        _inputActionCollections.Add(newInstance);
        return newInstance;
    }
}
