public interface IPriorityInteractable : IInteractable
{
    InputConfigSO InputConfigSO { get; }
    public InputActionEntry GetFirstEntry();
}