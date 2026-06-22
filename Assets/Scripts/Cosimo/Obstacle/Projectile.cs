using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Basic projectile. Using object pooler pattern.
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _lifetime;

    [SerializeField] private Vector2 _direction;
    private float _timer;

    private ObjectPooler<Projectile> _pool;
    private Tilemap _targetTilemap;
   
    internal void Initialize(Vector2 direction, ObjectPooler<Projectile> poolRef,Tilemap tilemap)
    {
        _direction = direction;
        _targetTilemap = tilemap;
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

        if(_targetTilemap != null && collision.gameObject==_targetTilemap.gameObject)
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


