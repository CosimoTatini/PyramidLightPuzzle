public interface ILightTriggerReceiver
{
    LightTrigger LightTrigger { get; }
    void SetLightTrigger(LightTrigger lightTrigger);
    void LightActivated();
    void LightChanged();
    void LightDeactivated();
}