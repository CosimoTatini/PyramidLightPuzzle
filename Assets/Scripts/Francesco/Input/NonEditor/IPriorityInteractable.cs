using UnityEngine;
using UnityEngine.Events;
public interface IPriorityInteractable : IInteractable
{
    UnityEvent OnInteract { get; }
    InputConfigSO InputConfigSO { get; }
    public InputActionEntry GetFirstEntry();
}