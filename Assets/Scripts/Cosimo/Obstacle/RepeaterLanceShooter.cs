using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RepeaterLanceShooter : MonoBehaviour, IObserver
{
    [Header("Pool & Spawn")]
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    private ObjectPooler<Projectile> _projectilePooler;

    [Header("Fisica")]
    [SerializeField] private float _launchForce = 10f; 

    [Header("Time Settings")]
    [Tooltip("Ogni quanto tempo la torretta esegue una raffica di spari")]
    [SerializeField] private float _cooldownBetweenBursts = 5f; 
    [Tooltip("Ritardo iniziale prima del primo sparo (per desincronizzare)")]
    [SerializeField] private float _shootingDelay = 0f; // Ogni quanto spara la torretta 
    [SerializeField] private int _projectilePerBurst = 3;
    [SerializeField] private float _delayBetweenShots = 0.2f;
    [Tooltip("Durata del proiettile di QUESTA specifica torretta")]
    [SerializeField] private float _projectileLifetime = 3f;

    [Header("Settings")]
    [SerializeField] private Vector2 _direction = new Vector2(-1, 0f);
    [SerializeField] private Tilemap _targetTilemap;

    private Coroutine _shootCoroutine;
    private bool _isFirstSpawn = true;

    private void Awake()
    {
        _projectilePooler = new ObjectPooler<Projectile>(_projectilePrefab, parent: transform);
    }

    private void Start()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.Attach(this);
        }

        _isFirstSpawn = true;
        _shootCoroutine = StartCoroutine(ShootingCoroutine());
    }

    private IEnumerator ShootingCoroutine()
    {
        
        if (_isFirstSpawn && _shootingDelay > 0f)
        {
            yield return new WaitForSeconds(_shootingDelay);
            _isFirstSpawn = false;
        }

        while (true)
        {
        
            for (int i = 0; i < _projectilePerBurst; i++)
            {
                SpawnProjectile();
                yield return new WaitForSeconds(_delayBetweenShots);
            }

            yield return new WaitForSeconds(_cooldownBetweenBursts);
        }
    }

    private void SpawnProjectile()
    {
        if (_firePoint == null) return;

        Projectile proj = _projectilePooler.Get();
        proj.transform.position = _firePoint.position;

        
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        proj.gameObject.SetActive(true);

        proj.Initialize(_projectilePooler, _targetTilemap, _projectileLifetime);

        
        Vector2 force = _direction.normalized * _launchForce;
        proj.Rb.AddForce(force, ForceMode2D.Impulse); 
    }

    public void ObserverUpdate(ISubject subject)
    {
        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
        }

        _isFirstSpawn = true;
        _shootCoroutine = StartCoroutine(ShootingCoroutine());
        Debug.Log("Torretta Resettata");
    }

    private void OnDestroy()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.Detach(this);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 startPoint = _firePoint != null ? _firePoint.position : transform.position;
        Vector3 direction3D = new Vector3(_direction.x, _direction.y, 0f).normalized;

        Gizmos.color = Color.red;
        float lineLength = 5f;
        Vector3 endPoint = startPoint + direction3D * lineLength;
        Gizmos.DrawLine(startPoint, endPoint);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPoint, 0.2f);
    }
#endif
}