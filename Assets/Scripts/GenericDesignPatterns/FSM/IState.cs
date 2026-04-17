/// <summary>
/// The base interface for a state machine on a monobehaviour
/// </summary>
public interface IState
{
    public void OnStart();
    public void OnEnd();
    public void OnUpdate();
    public void OnFixedUpdate();
}