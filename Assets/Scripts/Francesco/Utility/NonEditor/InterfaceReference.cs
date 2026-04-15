using UnityEngine;

[System.Serializable]
public abstract class InterfaceReference<T>
{
    [SerializeField] private GameObject _gameObject;

    /*TODO: Maybe also add caching values, this works fine as long as we don't add components at runtime,
    still finding the components each time isn't a big issue if we don't call it every frame and for a lots of objects,
    can also add a middleman component which we use to add/remove components, then we subscribe to it so we always know when to refresh
    our array, instead of searching every time
    */
    public virtual T[] Value
    {
        get
        {
            if (!_gameObject) return System.Array.Empty<T>();
            var components = _gameObject.GetComponents<T>();
            return components;
        }
    }
}