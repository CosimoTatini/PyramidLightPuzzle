using UnityEngine;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour
{
    private float _lifetime;
    private float _timer;
    private ObjectPooler<Projectile> _pool;

    [SerializeField] private LayerMask _layerMask;

    private Tilemap _targetTilemap;

    private Rigidbody2D _rb;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }


    public Rigidbody2D Rb => _rb;

    internal void Initialize(ObjectPooler<Projectile> poolRef, Tilemap tilemap, float lifetime)
    {
        _targetTilemap = tilemap;
        _pool = poolRef;
        _lifetime = lifetime;
        _timer = 0f;


        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }

    private void Update()
    {

        _timer += Time.deltaTime;
        if (_timer >= _lifetime)
        {
            ReturnToPool();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_layerMask.Contains(collision.gameObject.layer))
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player _))
        {
            ReturnToPool();
            return;
        }
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
        _pool.Set(this);
    }
}