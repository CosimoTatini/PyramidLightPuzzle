using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TriggerArea2D : MonoBehaviour
{
    [Tooltip("Only calls the events when a GameObject with any of these types triggers them")]
    [SerializeField] private List<TypeVar> _typeFilters = new();
    [SerializeField] private bool _onlySearchSelf = false;
    public UnityEvent OnTriggerEnter;
    public UnityEvent OnTriggerStay;
    public UnityEvent OnTriggerExit;
    public event Action<Collider2D> OnTriggerEnterAction;
    public event Action<Collider2D> OnTriggerStayAction;
    public event Action<Collider2D> OnTriggerExitAction;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HasAnyValidComponents(collision.gameObject))
        {
            OnTriggerEnterAction?.Invoke(collision);
            OnTriggerEnter.Invoke();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (HasAnyValidComponents(collision.gameObject))
        {
            OnTriggerStayAction?.Invoke(collision);
            OnTriggerStay.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (HasAnyValidComponents(collision.gameObject))
        {
            OnTriggerExitAction?.Invoke(collision);
            OnTriggerExit.Invoke();
        }
    }

    private bool HasAnyValidComponents(GameObject gameObject)
    {
        if (gameObject == null) return false;
        if (_typeFilters == null || _typeFilters.Count == 0) return true;
        var types = _typeFilters.Where(t => t != null && t.Type != null).Select(t => t.Type).ToArray();

        if (types.Length == 0) return true;

        Transform parent = gameObject.transform.parent;

        // search in the gameobject itself
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            if (gameObject.TryGetComponent(type, out _))
            {
                return true;
            }
        }

        if(_onlySearchSelf) return false;

        // search in the parent gameobject
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            if (parent != null)
            {
                if (parent.TryGetComponent(type, out _))
                {
                    return true;

                }
            }
        }

        // search in the direct children of the gameobject
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            foreach (Transform childTransform in gameObject.transform)
            {
                if (childTransform.TryGetComponent(type, out _))
                {
                    return true;
                }
            }
        }

        // wide search in the hierarchy up and down
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];

            if (gameObject.GetComponentInParent(type) != null)
            {
                return true;
            }
            if (gameObject.GetComponentInChildren(type) != null)
            {
                return true;
            }
        }

        // nothing found
        return false;
    }
}
