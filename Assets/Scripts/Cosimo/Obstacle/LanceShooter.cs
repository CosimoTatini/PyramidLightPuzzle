using System;
using System.Collections;
using UnityEngine;

public class LanceShooter : MonoBehaviour, IObserver
{
    [SerializeField] private ObjectPooler<Projectile> _pool;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private Vector2 _direction;
    [SerializeField] private BoxCollider2D _coll;
    [SerializeField] private GameObject gameObjectToDestroy;
    private Coroutine _disableCoroutine;

    public Projectile _prefabProjectile;
    private bool _isShooting;

    private void Awake()
    {
        _pool = new ObjectPooler<Projectile>(_prefabProjectile);
    }

    private void Start()
    {
        Player player = FindFirstObjectByType<Player>();

        if (player!=null)
        {
            player.Attach(this);
            Debug.Log($"[Trap] Registrazione effettuata con successo su {player.name}");
        }
        else
        {
            Debug.LogError("[Trap] Errore: Player non trovato in scena!");
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
             Shoot();
            _isShooting=true;
            if(_disableCoroutine==null)
            {
                StartCoroutine(DisableTriggerCoroutine());
            }
           
        }
    }
    
    private IEnumerator DisableTriggerCoroutine()
    {
       yield return null;
       _coll.enabled =false;
        _disableCoroutine = null;
    }

    public void ObserverUpdate(ISubject subject)
    {
        if (_disableCoroutine != null)
        {
            StopCoroutine(_disableCoroutine);
            _disableCoroutine = null;
        }
        _coll.enabled=true;
        Debug.Log("Trappola resettata");
    }

    private void OnDestroy()
    {
        Player player = FindFirstObjectByType<Player>();

        if( player != null )
        {
            player.Detach(this);
        }
    }

    //TODO : se il player è respawnato riattivo il collider
}
