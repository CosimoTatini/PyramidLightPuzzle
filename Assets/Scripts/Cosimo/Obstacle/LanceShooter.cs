using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Oneshot lance shooter
/// </summary>
public class LanceShooter : MonoBehaviour
{
    //[Header("Pool & Spawn")]
    //[SerializeField] private ObjectPooler<Projectile> _pool;
    //[SerializeField] private Transform _firePoint;
    //[SerializeField] private Projectile _prefabProjectile;

    //[Header("Settings")]
    //[SerializeField] private Vector2 _direction = new Vector2(-1f, 0f);
    //[SerializeField] private BoxCollider2D _coll;
    //[SerializeField] private Tilemap _targetTilemap;
    //[SerializeField] private GameObject gameObjectToDestroy; 
    //private Coroutine _disableCoroutine;
    //private float _lifetime;
    //private bool _isShooting;

    //private void Awake()
    //{
    //    _pool = new ObjectPooler<Projectile>(_prefabProjectile);
    //}

    //private void Start()
    //{
    //    Player player = FindFirstObjectByType<Player>();
    //    if (player != null)
    //    {
    //        player.Attach(this);
    //        Debug.Log($"[Trap] Registrazione effettuata con successo su {player.name}");
    //    }
    //    else
    //    {
    //        Debug.LogError("[Trap] Errore: Player non trovato in scena!");
    //    }
    //}

    //private void Shoot()
    //{
    //    Projectile proj = _pool.Get();
    //    proj.transform.position = _firePoint.position;
    //    proj.gameObject.SetActive(true);

       
    //    proj.Initialize(_direction, _pool, _targetTilemap,_lifetime);
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
        
    //    if (_isShooting) return;

    //    if (collision.TryGetComponent(out Player player))
    //    {
    //        _isShooting = true;
    //        Shoot();

    //        if (_disableCoroutine == null)
    //        {
    //            _disableCoroutine = StartCoroutine(DisableTriggerCoroutine());
    //        }
    //    }
    //}

    //private IEnumerator DisableTriggerCoroutine()
    //{
    //    yield return null;
    //    _coll.enabled = false;
    //    _disableCoroutine = null;
    //}

    //public void ObserverUpdate(ISubject subject)
    //{
    //    if (_disableCoroutine != null)
    //    {
    //        StopCoroutine(_disableCoroutine);
    //        _disableCoroutine = null;
    //    }

    //    _isShooting = false; 
    //    _coll.enabled = true;
    //    Debug.Log("Trappola resettata");
    //}

    //private void OnDestroy()
    //{
    //    Player player = FindFirstObjectByType<Player>();
    //    if (player != null)
    //    {
    //        player.Detach(this);
    //    }
    //}
}