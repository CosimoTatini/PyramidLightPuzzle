public interface IPriorityInteractableHost
{
    InteractableContextRegistry InteractableContextRegistry {get;}
    void AddInteractable(IPriorityInteractable priorityInteractable);
    void RemoveInteractable(IPriorityInteractable priorityInteractable);
    bool ContainsInteractable(IPriorityInteractable priorityInteractable);
}