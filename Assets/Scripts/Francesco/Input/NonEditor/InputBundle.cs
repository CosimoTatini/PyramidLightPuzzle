using System;
using System.Collections.Generic;
using System.Linq;
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
            if (action != null)
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
    public T GetInputSystemInstance<T>(out bool createdInstance) where T : class, IInputActionCollection2, new()
    {
        createdInstance = false;

        foreach (var inputActionCollection in _inputActionCollections)
        {
            if (inputActionCollection is T t)
            {
                return t;
            }
        }

        T newInstance = new T();
        _inputActionCollections.Add(newInstance);
        createdInstance = true;
        return newInstance;
    }

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

    public IInputActionCollection2 GetInputSystemInstance(Type type, out bool createdInstance)
    {
        createdInstance = false;

        // 1. Validation: Ensure the type implements the required interface
        if (!typeof(IInputActionCollection2).IsAssignableFrom(type))
        {
            Debug.LogError($"{type.Name} does not implement IInputActionCollection2");
            return null;
        }

        // 2. Search existing instances
        foreach (var inputActionCollection in _inputActionCollections)
        {
            if (inputActionCollection.GetType() == type)
            {
                return inputActionCollection;
            }
        }

        // 3. Create new instance via Reflection
        try
        {
            IInputActionCollection2 newInstance = (IInputActionCollection2)Activator.CreateInstance(type);
            newInstance.ElementAt(0);
            Debug.Log($"Created new instance of: {type.Name}");

            _inputActionCollections.Add(newInstance);
            createdInstance = true;
            return newInstance;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to instantiate {type.Name}: {e.Message}");
            return null;
        }
    }

    public IInputActionCollection2 GetInputSystemInstance(Type type)
    {
        // 1. Validation: Ensure the type implements the required interface
        if (!typeof(IInputActionCollection2).IsAssignableFrom(type))
        {
            Debug.LogError($"{type.Name} does not implement IInputActionCollection2");
            return null;
        }

        // 2. Search existing instances
        foreach (var inputActionCollection in _inputActionCollections)
        {
            if (inputActionCollection.GetType() == type)
            {
                return inputActionCollection;
            }
        }

        // 3. Create new instance via Reflection
        try
        {
            IInputActionCollection2 newInstance = (IInputActionCollection2)Activator.CreateInstance(type);
            // foreach (var item in newInstance)
            // {
            //     item.Disable();
            // }
            newInstance.ElementAt(0);
            Debug.Log($"Created new instance of: {type.Name}");

            _inputActionCollections.Add(newInstance);
            return newInstance;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to instantiate {type.Name}: {e.Message}");
            return null;
        }
    }

}
