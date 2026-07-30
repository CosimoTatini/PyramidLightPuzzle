using UnityEngine;
using UnityEngine.Events;

public class WalkSounds : MonoBehaviour
{
    public UnityEvent OnPlayerWalks;
    public void PlayerWalks()
    {
        OnPlayerWalks.Invoke();
    }

   
}
