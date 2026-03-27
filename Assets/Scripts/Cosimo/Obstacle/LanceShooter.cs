using System;
using System.Collections;
using UnityEngine;

public class LanceShooter : MonoBehaviour
{
    [SerializeField] private ObjectPooler<Projectile> _pool;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private Vector2 _direction;
    [SerializeField] private BoxCollider2D _coll;
    [SerializeField] private GameObject gameObjectToDestroy;

    public Projectile _prefabProjectile;
    private float _disableTriggerDelay = 1f;
    private bool _isShooting;

    private void Awake()
    {
        _pool = new ObjectPooler<Projectile>(_prefabProjectile);
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
             Shoot();
            _isShooting=true;
            StartCoroutine(DisableTriggerCoroutine());
        }
    }
    
    private IEnumerator DisableTriggerCoroutine()
    {
       yield return new WaitForSeconds(_disableTriggerDelay);
       _coll.enabled =false;     
    }
}
