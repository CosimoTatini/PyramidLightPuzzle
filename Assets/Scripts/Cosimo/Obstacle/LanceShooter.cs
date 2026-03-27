using System;
using UnityEngine;

public class LanceShooter : MonoBehaviour
{
    [SerializeField] private ObjectPooler<Projectile> _pool;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private Vector2 _direction;

    private bool _active;
    private float _timer;
    public Projectile _prefabProjectile;

    private void Awake()
    {
        _pool = new ObjectPooler<Projectile>(_prefabProjectile);
    }
    private void Update()
    {
        if (!_active) return;

        _timer += Time.deltaTime;

        if(_timer >= _fireRate)
        {
            Shoot();
            _timer = 0f;
        }
    }

    private void Shoot()
    {
        Projectile proj = _pool.Get();
        proj.transform.position = _firePoint.position;
        proj.gameObject.SetActive(true);

        proj.Initialize(_direction, _pool);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Player player))
        {
            _active = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Player player))
        {
            _active= false;
        }
    }
}
