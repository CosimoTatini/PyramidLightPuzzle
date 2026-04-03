public interface ILightTriggerReceiver
{
    void LightActivated(LightTrigger lightTrigger);
    void LightChanged(LightTrigger lightTrigger);
    void LightDeactivated(LightTrigger lightTrigger);
}