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

    [Header("Time Settings")]
    [SerializeField] private float _cooldownBeetweenBursts = 2f;
    [SerializeField] private int _projectilePerBurst = 3;
    [SerializeField] private float _delayBeetweenShots = 0.2f;

    [Header("Settings")]
    [SerializeField] private Vector2 _direction = new Vector2(-1, 0f);
    [SerializeField] private Tilemap _targetTilemap;

    private Coroutine _shootCoroutine;

    private void Awake()
    {
        _projectilePooler = new ObjectPooler<Projectile>(_projectilePrefab);
    }

    private void Start()
    {
        Player player = FindFirstObjectByType<Player>();

        if(player!=null)
        {
            player.Attach(this);
        }

        _shootCoroutine = StartCoroutine(ShootingCoroutine());
    }

    private IEnumerator ShootingCoroutine()
    {
       while(true)
        {
            for (int i = 0; i < _projectilePerBurst; i++)
            {
                SpawnProjectile();
                yield return new WaitForSeconds(_delayBeetweenShots);
            }
            yield return new WaitForSeconds(_cooldownBeetweenBursts);
        }
    }

    private void SpawnProjectile()
    {
        if (_firePoint == null) return;

        Projectile proj = _projectilePooler.Get();
        proj.transform.position= _firePoint.position;
        proj.gameObject.SetActive(true);
        proj.Initialize(_direction,_projectilePooler, _targetTilemap);
    }

    public void ObserverUpdate(ISubject subject)
    {
       if(_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
        }
        StartCoroutine(ShootingCoroutine());
        Debug.Log("Reset Done");
    }

    private void OnDestroy()
    {
        Player player = FindFirstObjectByType<Player>();
        if(player != null)
        {
            player.Detach(this);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 startPoint = _firePoint != null ? _firePoint.position : transform.position;

        // Calcoliamo la direzione (assicurandoci che sia un vettore normalizzato di lunghezza 1)
        Vector3 direction3D = new Vector3(_direction.x, _direction.y, 0f).normalized;

        // Impostiamo un colore visibile (es. Rosso per i proiettili)
        Gizmos.color = Color.red;

        // 1. Disegniamo la linea principale della traiettoria (lunga ad esempio 10 unit�)
        float lineLength = 10f;
        Vector3 endPoint = startPoint + direction3D * lineLength;
        Gizmos.DrawLine(startPoint, endPoint);

        // 2. Disegniamo una piccola sfera sul punto di sparo per identificarlo chiaramente
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPoint, 0.2f);
    }
#endif
}
