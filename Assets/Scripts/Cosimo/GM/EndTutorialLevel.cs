using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorialLevel : MonoBehaviour
{
    public void End()
    {
        GameManager.Instance.LoadPyramid();
    }
}
