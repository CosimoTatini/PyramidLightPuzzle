using UnityEngine;

public class MummyGrowls : MonoBehaviour
{
    [SerializeField] private AudioClipListGroup _audioClipListGroup;

    public void OnMummyGrowlSound()
    {
        Global2DAudioPlayer.Instance.PlayOneShotRandom(_audioClipListGroup);
    }
}
