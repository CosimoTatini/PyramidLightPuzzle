using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorialLevel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out var player))
        {
         LevelManager.Instance.ChangeScene("PyramidCosOfficial");
         LevelManager.Instance.SwitchGameplayScene("TutorialFra", "PyramidGabriele");
        }
       
    }
}
