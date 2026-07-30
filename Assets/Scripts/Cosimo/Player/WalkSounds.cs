using UnityEngine;

public class WalkSounds : MonoBehaviour
{


  public void OnFootStepsAnimSound()
  {
    Global2DAudioPlayer.Instance.PlayOneShotRandom(_audioClipListGroup);
    Debug.Log("Clip riprodotta" + _audioClipListGroup.AudioClips);
  }
}
