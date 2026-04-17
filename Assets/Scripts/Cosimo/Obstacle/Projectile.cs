using System;
using System.Threading;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _lifetime;

    [SerializeField] private Vector2 _direction;
    private float _timer;

    private ObjectPooler<Projectile> _pool;
    internal void Initialize(Vector2 direction, ObjectPooler<Projectile> poolRef)
    {
        _direction = direction;
        _pool = poolRef;
        _timer = 0f;
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
       _timer += Time.deltaTime;

        if(_timer >= _lifetime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out Player player))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
        _pool.Set(this);
    }
}


