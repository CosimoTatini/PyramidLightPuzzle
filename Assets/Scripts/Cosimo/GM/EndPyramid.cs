using UnityEngine;

public class EndPyramid : MonoBehaviour
{
    public void End()
    {
        GameManager.Instance.LoadEndGame();
    }
}
