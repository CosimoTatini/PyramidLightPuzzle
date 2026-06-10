using System;
using System.Collections;
using UnityEngine;

public class ActiveDeactiveObstacle : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField] private float _timeBeetweenActivaction = 1.0f;
    [SerializeField] private float _delayBeforeDeactivaction = 2.0f;
    [SerializeField] private float _timeBeetweenDeactivaction = 1.0f;

    private void Start()
    {
        StartCoroutine(ActivactionDeactivationCoroutine());
    }

    private IEnumerator ActivactionDeactivationCoroutine()
    {
        int childCount = transform.childCount;

        if(childCount == 0 )
        {
            yield break;
        }

        for (int i = 0; i < childCount; i++)
        {
            //Here i will play an anim that goes from down to up. The below row will be changed in Animator.Play(clipname);
            transform.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(_timeBeetweenActivaction);
            
        }

        yield return new WaitForSeconds(_delayBeforeDeactivaction);

        for(int i = childCount-1; i>=0; i--)
        {
            transform.GetChild(i).gameObject.SetActive(false);
            yield return new WaitForSeconds(_timeBeetweenDeactivaction);
        }


    }
}
