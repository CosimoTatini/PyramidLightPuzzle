using System;
using Unity.VisualScripting;
using UnityEngine;

public class FragilePlatform : MonoBehaviour
{
    [SerializeField] private float _maxTime = 2f;
    [SerializeField] private float _refreshTimerDuration = 1f;

    private float _currentTime;
    private bool _playerOn;

    private void Update()
    {
        if (_playerOn)
        {
            _currentTime += Time.deltaTime;
            if( _currentTime >= _maxTime )
            {
                BreakPlatform();
            }
        }

        else
        {
            _currentTime -= Time.deltaTime * _refreshTimerDuration;
            _currentTime = Mathf.Clamp(_currentTime, 0, _maxTime);
        }
    }

    private void BreakPlatform()
    {
        gameObject.SetActive( false );
         
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<Player>() != null)
        {
            _playerOn = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if( collision.gameObject.GetComponent<Player>())
        {
            _playerOn = false;
        }
    }
}
