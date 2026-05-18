using System;
using UnityEngine;

public class Mummy : MonoBehaviour
{
    //TODO: trasformerò l'Ia di questo nemico passando ai Behavior tree
    [Header("Patrol Settings")]
    public float MoveSpeed = 2f;
    public float LeftLimit = -5f;
    public float RightLimit = 5f;

    private bool _movingRight = true;
    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        float moveDir = _movingRight ? 1f : -1f;

        transform.Translate(Vector2.right * moveDir * MoveSpeed * Time.deltaTime);

        if(transform.position.x >= RightLimit && _movingRight)
        {
            _movingRight = false;
            Flip();
        }

        else if( transform.position.x <= LeftLimit != _movingRight )
        {
            _movingRight = true;
            Flip();
        }
    }

    private void Flip()
    {
        if(_renderer != null)
        {
            _renderer.flipX = !_renderer.flipX;

        }
    }
}
